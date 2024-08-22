using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Operations {
    class DateAdd : Node {
        public Parameter<DateParam> Date { get; } = new("date", true, new());

        public Parameter<EnumParameter> Timespan { get; } = new("timespan", false, new EnumParameter("m", new() {
            { "date_days", "d" },
            { "date_weeks", "w" },
            { "date_months", "m" },
            { "date_years", "y" }
        }));

        public Parameter<IntParam> Value { get; } = new("value", true, new());

        [JsonIgnore]
        public Result<DateParam> Result { get; } = new("date");

        protected override ENodeType NodeType => ENodeType.Parameter;

        public static string Id => "op_dateAdd";

        public override string NodeTypeId => Id;

        public DateAdd() {
            RegisterParameter(Date);
            RegisterParameter(Timespan);
            RegisterParameter(Value);

            RegisterResult(Result);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var date = Date.Value.ResolveDate();
            var value = Value.Value.Value;

            Logger.Debug($"Input date: {date}");

            switch (Timespan.Value.Value) {
                case "d":
                    date = date.AddDays(value);
                    break;
                case "w":
                    date = date.AddDays(value * 7);
                    break;
                case "m":
                    date = date.AddMonths(value);
                    break;
                case "y":
                    date = date.AddYears(value);
                    break;
            }

            Logger.Debug($"Output date: {date}");

            Result.Value = new(date);
            return true;
        }
    }
}