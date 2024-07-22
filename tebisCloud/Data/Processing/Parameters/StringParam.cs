using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data.Processing.Parameters {
    public class StringParam : ParamType {
        public string Value { get; set; } = "";

        public override ParamType Clone() {
            return new StringParam { Value = Value };
        }

        public override void Dispose() {
        }
    }
}