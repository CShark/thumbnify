using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Parameters {
    class DateFind : Node {
        public Parameter<DateParam> Date { get; } = new("date", true, new());

        public Parameter<EnumParameter> Find { get; } = new("timespan", false, new("w", new() {
            { "date_week", "w" },
            { "date_month", "m" },
            { "date_year", "y" }
        }));

        [JsonIgnore]
        public Result<DateParam> Result { get; } = new("date");

        protected override ENodeType NodeType => ENodeType.Parameter;
        public static string Id => "op_dateFind";
        public override string NodeTypeId => Id;

        public DateFind() {
            RegisterParameter(Date);
            RegisterParameter(Find);

            RegisterResult(Result);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var date = Date.Value.ResolveDate();

            Logger.Debug($"Input date: {date}");

            switch (Find.Value.Value) {
                case "w":
                    var dayOfWeek = date.DayOfWeek;
                    date = date.AddDays(-(int)dayOfWeek);
                    break;
                case "m":
                    date = new DateTime(new DateOnly(date.Year, date.Month, 1), TimeOnly.FromDateTime(date));
                    break;
                case "y":
                    date = new DateTime(new DateOnly(date.Year, 1, 1), TimeOnly.FromDateTime(date));
                    break;
            }

            Logger.Debug($"Output date: {date}");

            Result.Value = new(date);

            return true;
        }
    }
}