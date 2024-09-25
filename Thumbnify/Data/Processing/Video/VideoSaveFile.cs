using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Video {
    class VideoSaveFile : Node {
        [JsonIgnore]
        public Parameter<VideoFile> Video { get; } = new("video", true);

        public Parameter<FilePath> Path { get; } =
            new("path", true, new FilePath(FilePath.EPathMode.SaveFile, "MP4-Videos|*.mp4"));

        [JsonIgnore]
        public Result<FilePath> VideoResult { get; } = new("path");

        protected override ENodeType NodeType => ENodeType.Video;
        public override string NodeTypeId => Id;

        public static string Id => "video_save";

        public VideoSaveFile() {
            RegisterParameter(Video);
            RegisterParameter(Path);

            RegisterResult(VideoResult);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var filename = Path.Value.FileName;

            if (!filename.ToLower().EndsWith(".mp4")) {
                filename += ".mp4";
            }

            filename = FileTools.SanitizeFilename(filename);

            if (File.Exists(filename)) {
                File.Delete(filename);
            }

            using (var src = new FileStream(Video.Value.VideoFileName, FileMode.Open)) {
                using (var dest = new FileStream(filename, FileMode.Create)) {
                    FileTools.CopyStreams(src, dest, ReportProgress, CancelToken, 1024 * 1024);
                }
            }

            VideoResult.Value = new FilePath(filename );

            return true;
        }
    }
}