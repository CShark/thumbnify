using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Operations {
    internal sealed class PathCombine : Node {
        public Parameter<FilePath> SourcePath { get; } =
            new("path", true, new FilePath(FilePath.EPathMode.Directory, ""));

        public Parameter<StringParam> Combine { get; } = new("value", true, new StringParam());

        [JsonIgnore]
        public Result<FilePath> ResultPath { get; } = new("path");

        public PathCombine() {
            RegisterParameter(SourcePath);
            RegisterParameter(Combine);

            RegisterResult(ResultPath);
        }

        protected override ENodeType NodeType => ENodeType.Parameter;
        public override string NodeTypeId => Id;

        public static string Id = "op_pathcombine";

        protected override bool Execute(CancellationToken cancelToken) {
            ResultPath.Value = new FilePath(FilePath.EPathMode.Directory, "") {
                FileName = Path.Combine(SourcePath.Value.FileName, Combine.Value.Value)
            };

            return true;
        }
    }
}