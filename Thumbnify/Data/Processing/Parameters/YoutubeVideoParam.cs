using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using YtVideo = Google.Apis.YouTube.v3.Data.Video;

namespace Thumbnify.Data.Processing.Parameters {
    internal class YoutubeVideoParam : ParamType {
        public class VideoMetadata {
            public string Id { get; }
            public string PrivacyStatus { get; }
            public DateTimeOffset? PublishedAt { get; }
            public string Title { get; }

            public VideoMetadata(YtVideo video) {
                Id = video.Id;
                Title = video.Snippet.Title;
                PrivacyStatus = video.Status.PrivacyStatus;
                PublishedAt = video.Status.PublishAtDateTimeOffset;
            }

            public VideoMetadata(PlaylistItem item) {
                Id = item.ContentDetails.VideoId;
                Title = item.Snippet.Title;
                PrivacyStatus = item.Status.PrivacyStatus;
                PublishedAt = item.ContentDetails.VideoPublishedAtDateTimeOffset;
            }
        }

        public List<VideoMetadata> Videos { get; set; } = new();

        public UserCredential Credentials { get; set; }

        public override ParamType Clone() {
            return new YoutubeVideoParam {
                Videos = Videos,
                Credentials = Credentials
            };
        }

        public override void Dispose() {
        }
    }
}