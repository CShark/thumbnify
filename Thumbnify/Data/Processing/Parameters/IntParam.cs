using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    public class IntParam : ParamType {
        public int Value { get; set; }

        public override ParamType Clone() {
            return new IntParam { Value = Value };
        }

        public override void Dispose() {
        }
    }
}