using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data.Processing.Parameters {
    class ThumbnailParam : ParamType {
        public ThumbnailData Thumbnail { get; set; }

        public override ParamType Clone() {
            return new ThumbnailParam {
                Thumbnail = Thumbnail
            };
        }

        public override void Dispose() {
        }
    }
}