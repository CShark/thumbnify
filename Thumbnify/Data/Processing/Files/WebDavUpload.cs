using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;
using WebDav;

namespace Thumbnify.Data.Processing.Files {
    class WebDavUpload : Node {
        public Parameter<FilePath> Source = new("file", true, new(FilePath.EPathMode.OpenFile, "All Files|*.*"));

        public Parameter<StringParam> WebDavUrl = new("url", true, new());

        public Parameter<StringParam> WebDavPath = new("path", true, new());

        public Parameter<CredentialParam> Credentials = new("credentials", true, new());

        protected override ENodeType NodeType => ENodeType.Parameter;
        public static string Id => "file_webdav";

        public override string NodeTypeId => Id;

        public WebDavUpload() {
            RegisterParameter(Source);
            RegisterParameter(WebDavUrl);
            RegisterParameter(WebDavPath);
            RegisterParameter(Credentials);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var handler = new HttpClientHandler();
            handler.Credentials = Credentials.Value.BuildCredentials();
            handler.PreAuthenticate = true;

            var baseUrl = WebDavUrl.Value.Value;

            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            var progress = new ProgressMessageHandler(handler);
            var client = new HttpClient(progress) {
                BaseAddress = new Uri(baseUrl),
                Timeout = new TimeSpan(0, 10, 0)
            };

            var webDav = new WebDavClient(client);
            var path = WebDavPath.Value.Value;
            var pathParts = path.Split('/').Where(x => !string.IsNullOrWhiteSpace(x));
            path = "";

            foreach (var part in pathParts) {
                var res = webDav.Propfind(path).Result;
                if (path == "") {
                    path = part;
                } else {
                    path += "/" + part;
                }

                var prop = res.Resources.FirstOrDefault(x => x.Uri.EndsWith(path + "/"));

                if (prop == null) {
                    webDav.Mkcol(path);
                    Logger.Debug($"Created new folder: {path}");
                }
            }

            var fileName = path + "/" + Path.GetFileName(Source.Value.FileName);

            progress.HttpSendProgress += (_, args) => {
                if (args.TotalBytes != null && args.TotalBytes > 0) {
                    ReportProgress(args.BytesTransferred, args.TotalBytes.Value);
                }

                if (cancelToken.IsCancellationRequested) {
                    client.CancelPendingRequests();
                }
            };

            using (var file = new FileStream(Source.Value.FileName, FileMode.Open)) {
                var result = webDav.PutFile(fileName, file).Result;

                if (!result.IsSuccessful) {
                    Logger.Error($"WebDav-Upload failed with Status Code {result.StatusCode}: {result.Description}");
                }

                return result.IsSuccessful;
            }
        }
    }
}