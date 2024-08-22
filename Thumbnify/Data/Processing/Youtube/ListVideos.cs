using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Youtube {
    internal class ListVideos :Node {
        [JsonIgnore]
        public Parameter<YoutubeVideoParam> Videos { get; } = new("videos", true);

        protected override ENodeType NodeType => ENodeType.Parameter;

        public static string Id = "op_printVideoList";
        public override string NodeTypeId => Id;

        public ListVideos() {
            RegisterParameter(Videos);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var videoList = string.Join("\r\n", Videos.Value.Videos.Select(x => $"{x.Title} ({x.Id})"));

            Logger.Debug($"Video List ({Videos.Value.Videos.Count}):\r\n{videoList}");

            return true;
        }
    }
}
