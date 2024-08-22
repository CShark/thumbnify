using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Youtube {
    internal class YoutubeRemPlaylist : Node {
        public Parameter<YoutubeVideoParam> Video { get; } = new("video", true);

        public Parameter<YoutubePlaylistParam> Playlist { get; } = new("playlist", true, new());

        protected override ENodeType NodeType => ENodeType.Youtube;
        public static string Id => "youtube_remplaylist";
        public override string NodeTypeId => Id;

        public YoutubeRemPlaylist() {
            RegisterParameter(Video);
            RegisterParameter(Playlist);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var cred = Playlist.Value.Credentials.ResolveCredentials().Result;

            if (cred == null) {
                Logger.Error("Could not resolve credentials for playlist");
                return false;
            }

            var service = new YouTubeService(new BaseClientService.Initializer {
                HttpClientInitializer = cred,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
            });

            var itemReq = service.PlaylistItems.List(new(["id", "contentDetails"]));
            itemReq.PlaylistId = Playlist.Value.PlaylistId;
            var itemIds = new List<string>();

            do {
                var items = itemReq.ExecuteAsync().Result;
                itemIds.AddRange(items.Items.Where(x => Video.Value.Videos.Any(y => y.Id == x.ContentDetails.VideoId))
                    .Select(x => x.Id));
                itemReq.PageToken = items.NextPageToken;
            } while (itemReq.PageToken != null);


            foreach (var item in itemIds) {
                var req = service.PlaylistItems.Delete(item);
                var result = req.ExecuteAsync().Result;
            }

            return true;
        }
    }
}