using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing {
    class FilterVideoDate :Node {
        [JsonIgnore]
        public Parameter<YoutubeVideoParam> Videos = new("videos", true);

        public Parameter<DateParam> Date = new("date", true, new());

        public Parameter<EnumParameter> FilterType = new("filter", false, new("before", new() {
            { "filter_before", "before" },
            { "filter_after", "after" }
        }), false);

        [JsonIgnore]
        public Result<YoutubeVideoParam> Result = new("videos");

        protected override ENodeType NodeType => ENodeType.Youtube;
        public static string Id => "op_filterDate";
        public override string NodeTypeId => Id;

        public FilterVideoDate() {
            RegisterParameter(Videos);
            RegisterParameter(Date);
            RegisterParameter(FilterType);

            RegisterResult(Result);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            Result.Value = new YoutubeVideoParam {
                Credentials = Videos.Value.Credentials
            };

            switch (FilterType.Value.Value) {
                case "before":
                    Result.Value.Videos = Videos.Value.Videos
                        .Where(x => x.PublishedAt < Date.Value.ResolveDate()).ToList();
                    break;
                case "after":
                    Result.Value.Videos = Videos.Value.Videos
                        .Where(x => x.PublishedAt > Date.Value.ResolveDate()).ToList();
                    break;
            }

            return true;
        }
    }
}
