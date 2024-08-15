using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Input {
    class TempPathInput : Node {
        [JsonIgnore]
        public Result<FilePath> Path = new("path");

        protected override ENodeType NodeType => ENodeType.Parameter;
        public static string Id => "input_temppath";
        public override string NodeTypeId => Id;

        public TempPathInput() {
            RegisterResult(Path);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            Path.Value = new FilePath(TempPath);
            return true;
        }
    }
}