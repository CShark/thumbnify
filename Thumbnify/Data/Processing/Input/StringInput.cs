using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Input {
    internal sealed class StringInput : Node {
        [JsonIgnore]
        public Result<StringParam> Result { get; } = new("value");

        public Parameter<StringParam> Input { get; } = new("value", false, new StringParam(), false);

        public StringInput() {
            RegisterParameter(Input);
            RegisterResult(Result);
        }

        protected override ENodeType NodeType => ENodeType.Parameter;
        public override string NodeTypeId => Id;
        public static string Id => "input_text";

        protected override bool Execute(CancellationToken cancelToken) {
            Result.Value = Input.Value;
            return true;
        }
    }
}