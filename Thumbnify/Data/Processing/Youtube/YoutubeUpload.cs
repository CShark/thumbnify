using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;
using YoutubeVideo = Google.Apis.YouTube.v3.Data.Video;

namespace Thumbnify.Data.Processing.Youtube {
    class YoutubeUpload : Node {
        [JsonIgnore]
        public Parameter<VideoFile> Video { get; } = new("video", true);

        public Parameter<YoutubeCredentialsParam> Credentials { get; } = new("account", false, new());

        public Parameter<StringParam> Title { get; } = new("title", true, new());

        public Parameter<StringParam> Description { get; } = new("description", true, new(true));

        public Parameter<EnumParameter> Status { get; } = new("status", false, new("private", new() {
            { "yt_private", "private" },
            { "yt_unlisted", "unlisted" },
            { "yt_public", "public" }
        }), false);

        public Parameter<EnumParameter> Category { get; } = new("category", false, new("1", new() {
            { "yt_cat_01", "1" },
            { "yt_cat_02", "2" },
            { "yt_cat_10", "10" },
            { "yt_cat_15", "15" },
            { "yt_cat_17", "17" },
            { "yt_cat_18", "18" },
            { "yt_cat_19", "19" },
            { "yt_cat_20", "20" },
            { "yt_cat_21", "21" },
            { "yt_cat_22", "22" },
            { "yt_cat_23", "23" },
            { "yt_cat_24", "24" },
            { "yt_cat_25", "25" },
            { "yt_cat_26", "26" },
            { "yt_cat_27", "27" },
            { "yt_cat_28", "28" },
            { "yt_cat_29", "29" },
            { "yt_cat_30", "30" },
            { "yt_cat_31", "31" },
            { "yt_cat_32", "32" },
            { "yt_cat_33", "33" },
            { "yt_cat_34", "34" },
            { "yt_cat_35", "35" },
            { "yt_cat_36", "36" },
            { "yt_cat_37", "37" },
            { "yt_cat_38", "38" },
            { "yt_cat_39", "39" },
            { "yt_cat_40", "40" },
            { "yt_cat_41", "41" },
            { "yt_cat_42", "42" },
            { "yt_cat_43", "43" },
            { "yt_cat_44", "44" },
        }));

        public Parameter<ThumbnailParam> Thumbnail { get; } = new("thumbnail", true, new());

        [JsonIgnore]
        public Result<YoutubeVideoParam> VideoResult = new("video");

        protected override ENodeType NodeType => ENodeType.Youtube;
        public static string Id => "youtube_upload";
        public override string NodeTypeId => Id;

        public YoutubeUpload() {
            RegisterParameter(Video);
            RegisterParameter(Credentials);
            RegisterParameter(Title);
            RegisterParameter(Description);
            RegisterParameter(Status);
            RegisterParameter(Category);
            RegisterParameter(Thumbnail);

            RegisterResult(VideoResult);
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

            var video = new YoutubeVideo {
                Snippet = new() {
                    Title = Title.Value.Value,
                    Description = Description.Value.Value,
                    CategoryId = Category.Value.Value,
                },
                Status = new() {
                    PrivacyStatus = Status.Value.Value
                }
            };

            YoutubeVideo? videoResult = null;
            using (var file = new FileStream(Video.Value.VideoFileName, FileMode.Open)) {
                var req = service.Videos.Insert(video, "snippet,status", file, "video/*");
                req.ProgressChanged += progress => { ReportProgress(progress.BytesSent, file.Length); };
                req.ResponseReceived += vid => { videoResult = vid; };

                req.UploadAsync(cancelToken).Wait(cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return false;

            var paramList = RequestParameters();
            App.Current.Dispatcher.Invoke(() => {
                var thumbnail = Thumbnail.Value.RenderThumbnail(paramList);
                var encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 70;
                encoder.Frames.Add(BitmapFrame.Create(thumbnail));

                using (var stream = new MemoryStream()) {
                    encoder.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    var req = service.Thumbnails.Set(videoResult.Id, stream, "image/jpeg");
                    req.UploadAsync(cancelToken).Wait(cancelToken);
                }
            });

            if (cancelToken.IsCancellationRequested) return false;

            VideoResult.Value = new YoutubeVideoParam {
                Credentials = cred,
                Videos = [new(videoResult)]
            };

            return true;
        }
    }
}