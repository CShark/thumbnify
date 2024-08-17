using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Operations {
    class RenderThumbnail : Node {
        public Parameter<ThumbnailParam> Thumbnail { get; } = new("thumbnail", true, new());

        public Parameter<FilePath> ImagePath { get; } =
            new("path", true, new(FilePath.EPathMode.SaveFile, "JPEG|*.jpeg"));

        [JsonIgnore]
        public Result<FilePath> FinalPath { get; } = new("path");

        protected override ENodeType NodeType => ENodeType.Parameter;
        public static string Id => "op_renderThumb";
        public override string NodeTypeId => Id;

        public RenderThumbnail() {
            RegisterParameter(Thumbnail);
            RegisterParameter(ImagePath);

            RegisterResult(FinalPath);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var path = ImagePath.Value.FileName;
            if (!path.ToLower().EndsWith(".jpg") && !path.ToLower().EndsWith(".jpeg")) {
                path += ".jpg";
            }

            App.Current.Dispatcher.Invoke(() => {
                var img = Thumbnail.Value.RenderThumbnail(RequestParameters());

                using (var stream = new FileStream(path, FileMode.Create)) {
                    var encoder = new JpegBitmapEncoder();
                    encoder.QualityLevel = 70;
                    encoder.Frames.Add(BitmapFrame.Create(img));
                    encoder.Save(stream);
                }
            });

            FinalPath.Value = new FilePath(path);
            return true;
        }
    }
}