using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    public class VideoFile : ParamType {
        public string VideoFileName { get; set; } = "";

        public override ParamType Clone() {
            // TODO: Copy file
            return new VideoFile { VideoFileName = VideoFileName };
        }

        public override void Dispose() {
            // TODO: Delete file
        }
    }
}