using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.ParamStore;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Operations {
    public class TextReplace : Node {
        public Parameter<StringParam> Input { get; } = new("text", true, new(true));

        [JsonIgnore]
        public Result<StringParam> Output { get; } = new("text");


        private static Regex _previewRegex = new(@"\{(.*?)\}", RegexOptions.Compiled);

        protected override ENodeType NodeType => ENodeType.Parameter;

        public static string Id => "op_textReplace";

        public override string NodeTypeId => Id;

        public TextReplace() {
            RegisterParameter(Input);
            RegisterResult(Output);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            Output.Value = new() {
                Value = ReplaceVariables(Input.Value.Value, RequestParameters())
            };

            return true;
        }

        public static string ReplaceVariables(string source, IList<ParamDefinition>? variables) {
            if (variables == null) {
                return source;
            } else {
                return _previewRegex.Replace(source, match => {
                    var value = match.Groups[1].Value;
                    var parts = value.Split('|');

                    var param = variables.FirstOrDefault(x =>
                        string.Equals(x.Name, parts[0], StringComparison.CurrentCultureIgnoreCase));

                    if (param != null) {
                        switch (param.Value) {
                            case StringParam s:
                                return s.Value;
                            case DateParam d:
                                if (parts.Length == 2) {
                                    return d.ResolveDate().ToString(parts[1]);
                                } else {
                                    return d.ResolveDate().ToString();
                                }
                        }
                    }

                    return "";
                });
            }
        }
    }
}