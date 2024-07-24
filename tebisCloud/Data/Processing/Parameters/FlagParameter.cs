using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    public class FlagParameter : ParamType {
        public bool Value { get; set; }
        public override ParamType Clone() {
            return new FlagParameter { Value = Value };
        }

        public override void Dispose() {
        }
    }
}