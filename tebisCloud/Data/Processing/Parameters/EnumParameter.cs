using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    internal class EnumParameter : ParamType {
        public string Value { get; set; }

        public Dictionary<string, string> Options { get; }

        public override ParamType Clone() {
            return new EnumParameter(Value, Options);
        }

        public override void Dispose() {
        }

        public EnumParameter(string value, Dictionary<string, string> options) {
            Value = value;
            Options = options;
        }
    }
}