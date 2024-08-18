using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Operations
{
    class TextCombine:Node {
        public Parameter<StringParam> Prefix = new("prefix", true, new(true));

        public Parameter<StringParam> Suffix = new("suffix", true, new(true));

        [JsonIgnore]
        public Result<StringParam> Result = new("text");

        protected override ENodeType NodeType => ENodeType.Parameter;
        public static string Id => "op_stringCombine";
        public override string NodeTypeId => Id;

        public TextCombine() {
            RegisterParameter(Prefix);
            RegisterParameter(Suffix);
            RegisterResult(Result);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            Result.Value = new() {
                Value = Prefix.Value.Value + Suffix.Value.Value
            };

            return true;
        }
    }
}
