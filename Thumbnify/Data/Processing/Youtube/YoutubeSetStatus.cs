using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Youtube {
    internal class YoutubeSetStatus : Node {
        [JsonIgnore]
        public Parameter<YoutubeVideoParam> Videos = new("videos", true);
        public Parameter<EnumParameter> Status { get; } = new("status", false, new("private", new() {
            { "yt_private", "private" },
            { "yt_unlisted", "unlisted" },
            { "yt_public", "public" }
        }), false);

        protected override ENodeType NodeType => ENodeType.Youtube;
        public static string Id = "youtube_setStatus";
        public override string NodeTypeId => Id;

        public YoutubeSetStatus() {
            RegisterParameter(Videos);
            RegisterParameter(Status);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var cred = Videos.Value.Credentials;

            var service = new YouTubeService(new BaseClientService.Initializer {
                HttpClientInitializer = cred,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
            });

            foreach (var video in Videos.Value.Videos) {
                var data = new Google.Apis.YouTube.v3.Data.Video();
                data.Id = video.Id;
                data.Status = new() {
                    PrivacyStatus = Status.Value.Value
                };

                var req = service.Videos.Update(data, "status");
                data = req.ExecuteAsync().Result;
            }

            return true;
        }
    }
}