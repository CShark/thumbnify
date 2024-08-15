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
    internal class YoutubeAddPlaylist :Node {

        public Parameter<YoutubeVideoParam> Video { get; } = new("video", true);

        public Parameter<YoutubePlaylistParam> Playlist { get; } = new("playlist", true, new());

        protected override ENodeType NodeType => ENodeType.Youtube;
        public static string Id => "youtube_addplaylist";
        public override string NodeTypeId => Id;

        public YoutubeAddPlaylist() {
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

            var item = new PlaylistItem {
                Snippet = new() {
                    PlaylistId = Playlist.Value.PlaylistId,
                    ResourceId = new() {
                        VideoId = Video.Value.Video.Id
                    } 
                }
            };

            var req = service.PlaylistItems.Insert(item, "snippet");

            item = req.ExecuteAsync().Result;

            return true;
        }
    }
}
