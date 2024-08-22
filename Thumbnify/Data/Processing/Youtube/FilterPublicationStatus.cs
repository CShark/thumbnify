using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Youtube {
    internal class FilterPublicationStatus : Node {
        [JsonIgnore]
        public Parameter<YoutubeVideoParam> Videos { get; } = new("videos", true);

        public Parameter<EnumParameter> Status { get; } = new("status", false, new("private", new() {
            { "yt_private", "private" },
            { "yt_unlisted", "unlisted" },
            { "yt_public", "public" }
        }), false);

        [JsonIgnore]
        public Result<YoutubeVideoParam> Result { get; } = new("videos");

        protected override ENodeType NodeType => ENodeType.Youtube;
        public static string Id => "op_filterPublication";
        public override string NodeTypeId => Id;

        public FilterPublicationStatus() {
            RegisterParameter(Videos);
            RegisterParameter(Status);

            RegisterResult(Result);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            Result.Value = new() {
                Credentials = Videos.Value.Credentials,
                Videos = Videos.Value.Videos.Where(x => x.PrivacyStatus == Status.Value.Value).ToList()
            };

            return true;
        }
    }
}