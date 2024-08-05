using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Converters {
    class ConvertDate : Node {
        [JsonIgnore]
        public Result<StringParam> String { get; } = new("date");

        public Parameter<StringParam> Format { get; } = new("format", true,
            new() {
                Value = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern + " " +
                        Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortTimePattern
            });

        public Parameter<DateParam> Date { get; } = new("date", true, new() { Today = true });

        protected override ENodeType NodeType => ENodeType.Parameter;
        public static string Id => "convert_date";
        public override string NodeTypeId => Id;

        public ConvertDate() {
            RegisterParameter(Date);
            RegisterParameter(Format);

            RegisterResult(String);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            String.Value = new() { Value = Date.Value.ResolveDate().ToString(Format.Value.Value) };

            return true;
        }
    }
}