using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using tebisCloud.Data.Processing.Parameters;
using tebisCloud.Postprocessing;

namespace tebisCloud.Data.Processing.Video {
    class VideoSaveFile : Node {
        public Parameter<VideoFile> Video { get; } = new("video", true);

        public Parameter<FilePath> Path { get; } =
            new("path", true, new FilePath(FilePath.EPathMode.SaveFile, "MP4-Videos|*.mp4"));

        protected override ENodeType NodeType => ENodeType.Video;
        protected override string NodeId => Id;

        public static string Id => "video_save";

        public VideoSaveFile() {
            RegisterParameter(Video);
            RegisterParameter(Path);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            if (File.Exists(Path.Value.FileName)) {
                File.Delete(Path.Value.FileName);
            }

            File.Copy(Video.Value.VideoFileName, Path.Value.FileName);

            return true;
        }
    }
}