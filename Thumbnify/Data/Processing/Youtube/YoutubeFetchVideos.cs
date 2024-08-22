using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;
using YoutubeVideo = Google.Apis.YouTube.v3.Data.Video;

namespace Thumbnify.Data.Processing.Youtube {
    class YoutubeFetchVideos : Node {
        public Parameter<YoutubeCredentialsParam> Credentials { get; } = new("credentials", true, new());

        public Result<YoutubeVideoParam> Videos { get; } = new("videos");

        protected override ENodeType NodeType => ENodeType.Youtube;
        public static string Id => "youtube_fetchVideo";
        public override string NodeTypeId => Id;

        public YoutubeFetchVideos() {
            RegisterParameter(Credentials);
            RegisterResult(Videos);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var cred = Credentials.Value.ResolveCredentials().Result;

            if (cred == null) {
                Logger.Error("Youtube credentials could not be initialized");
                return false;
            }

            var service = new YouTubeService(new BaseClientService.Initializer {
                HttpClientInitializer = cred,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
            });

            var channelReq = service.Channels.List("contentDetails");
            channelReq.Mine = true;
            var channel = channelReq.ExecuteAsync().Result;
            var uploads = channel.Items[0].ContentDetails.RelatedPlaylists.Uploads;

            var uploadReq = service.PlaylistItems.List(new(["contentDetails", "snippet", "status"]));
            uploadReq.PlaylistId = uploads;
            uploadReq.MaxResults = 50;
            var videos = new List<PlaylistItem>();

            do {
                var list = uploadReq.ExecuteAsync().Result;

                videos.AddRange(list.Items);

                uploadReq.PageToken = list.NextPageToken;

                if (cancelToken.IsCancellationRequested) return false;
            } while (uploadReq.PageToken != null);

            Videos.Value = new YoutubeVideoParam {
                Videos = videos.Select(x => new YoutubeVideoParam.VideoMetadata(x)).ToList(),
                Credentials = cred
            };

            return true;
        }
    }
}