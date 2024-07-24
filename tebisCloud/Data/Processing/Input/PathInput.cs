using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Input {
    internal sealed class PathInput : Node {
        public Parameter<FilePath> Path { get; } = new("path", false, new(FilePath.EPathMode.Directory, ""), false);

        [JsonIgnore]
        public Result<FilePath> Result { get; } = new("path");

        public PathInput() {
            RegisterParameter(Path);
            RegisterResult(Result);
        }

        protected override ENodeType NodeType => ENodeType.Parameter;
        protected override string NodeId => Id;

        public static string Id = "input_path";

        protected override bool Execute(CancellationToken cancelToken) {
            Result.Value = Path.Value;
            return true;
        }
    }
}