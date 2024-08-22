using System;
using System.Collections.Generic;
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
    class FilterPlaylist : Node {
        [JsonIgnore]
        public Parameter<YoutubeVideoParam> Videos { get; } = new("videos", true);

        public Parameter<YoutubePlaylistParam> Playlist { get; } = new("playlist", false, new());

        public Parameter<EnumParameter> FilterType { get; } = new("filter", false, new("0", new() {
            { "filter_contain", "0" },
            { "filter_noContain", "1" }
        }), false);

        [JsonIgnore]
        public Result<YoutubeVideoParam> Result { get; } = new("videos");

        protected override ENodeType NodeType => ENodeType.Youtube;
        public static string Id => "op_filterPlaylist";
        public override string NodeTypeId => Id;

        public FilterPlaylist() {
            RegisterParameter(Videos);
            RegisterParameter(Playlist);
            RegisterParameter(FilterType);

            RegisterResult(Result);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var cred = Playlist.Value.Credentials.ResolveCredentials().Result;
            var flip = FilterType.Value.Value == "1";

            var service = new YouTubeService(new BaseClientService.Initializer {
                HttpClientInitializer = cred,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name,
            });

            var playlist = service.PlaylistItems.List("contentDetails");
            playlist.PlaylistId = Playlist.Value.PlaylistId;
            playlist.MaxResults = 50;

            var playlistVideos = new List<string>();

            do {
                var result = playlist.ExecuteAsync().Result;

                playlistVideos.AddRange(result.Items.Select(x => x.ContentDetails.VideoId));
                playlist.PageToken = result.NextPageToken;
                if (cancelToken.IsCancellationRequested) return false;
            } while (playlist.PageToken != null);

            Result.Value = new YoutubeVideoParam() {
                Credentials = Videos.Value.Credentials,
                Videos = Videos.Value.Videos.Where(x => playlistVideos.Contains(x.Id) ^ flip).ToList()
            };

            return true;
        }
    }
}