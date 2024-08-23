using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Input {
    internal class DateInput :Node {
        public Parameter<DateParam> Date = new("date", false, new(), false);

        [JsonIgnore]
        public Result<DateParam> Result { get; } = new("date");

        protected override ENodeType NodeType => ENodeType.Parameter;

        public static string Id => "input_date";
        public override string NodeTypeId => Id;

        public DateInput() {
            RegisterParameter(Date);
            RegisterResult(Result);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            Result.Value = Date.Value;
            return true;
        }
    }
}
