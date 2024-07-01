using System.IO;
using Newtonsoft.Json;
using tebisCloud.Data.Processing.Parameters;
using tebisCloud.Postprocessing;

namespace tebisCloud.Data.Processing.Operations {
    internal class PathCombine : Node {
        public override IReadOnlyDictionary<string, Parameter> Parameters { get; protected set; }
        public override IReadOnlyDictionary<string, Result> Results { get; protected set; }

        public Parameter<FilePath> SourcePath { get; set; } =
            new("path", "Pfad", true, new FilePath(FilePath.EPathMode.Directory, ""));

        public Parameter<StringParam> Combine { get; set; } = new("suffix", "Sub-Pfad", true, new StringParam());

        [JsonIgnore]
        public Result<FilePath> ResultPath { get; set; } = new Result<FilePath>("path", "Pfad");

        public PathCombine() {
            Initialize();
        }

        protected override void InitializeParamsResults() {
            Parameters = new Dictionary<string, Parameter> {
                { SourcePath.Id, SourcePath },
                { Combine.Id, Combine }
            };

            Results = new Dictionary<string, Result> {
                { ResultPath.Id, ResultPath }
            };
        }

        protected override bool Execute(CancellationToken cancelToken) {
            ResultPath.Value = new FilePath(FilePath.EPathMode.Directory, "") {
                FileName = Path.Combine(SourcePath.Value.FileName, Combine.Value.Value)
            };

            return true;
        }

        public override EditorNode GenerateNode() {
            return new EditorNode("Pfad kombinieren", ENodeType.Parameter, this);
        }
    }
}