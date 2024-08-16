using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Input {
    internal class ThumbnailInput :Node {
        public Parameter<ThumbnailParam> Thumbnail { get; } = new("thumbnail", false, new(), false);

        [JsonIgnore]
        public Result<ThumbnailParam> ThumbnailResult { get; } = new("thumbnail");

        protected override ENodeType NodeType => ENodeType.Parameter;
        public static string Id => "input_thumbnail";
        public override string NodeTypeId => Id;

        public ThumbnailInput() {
            RegisterParameter(Thumbnail);
            RegisterResult(ThumbnailResult);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            ThumbnailResult.Value = Thumbnail.Value;
            return true;
        }
    }
}
