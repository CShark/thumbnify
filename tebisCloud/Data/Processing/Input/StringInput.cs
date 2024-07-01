using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tebisCloud.Data.Processing.Parameters;
using tebisCloud.Postprocessing;

namespace tebisCloud.Data.Processing.Input {
    internal class StringInput :Node{
        public override IReadOnlyDictionary<string, Parameter> Parameters { get; protected set; }
        public override IReadOnlyDictionary<string, Result> Results { get; protected set; }

        [JsonIgnore]
        public Result<StringParam> Result { get; set; } = new("value", "Wert");

        public Parameter<StringParam> Input { get; set; } = new("value", "Wert", false, new StringParam(), false);

        public StringInput() {
            Initialize();
        }

        protected override void InitializeParamsResults() {
            Parameters = new Dictionary<string, Parameter> {
                { Input.Id, Input }
            };

            Results = new Dictionary<string, Result> {
                { Result.Id, Result }
            };
        }

        protected override bool Execute(CancellationToken cancelToken) {
            Result.Value = Input.Value;
            return true;
        }

        public override EditorNode GenerateNode() {
            return new("Text", ENodeType.Parameter, this);
        }
    }
}
