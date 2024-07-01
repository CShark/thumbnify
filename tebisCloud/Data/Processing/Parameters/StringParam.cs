using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data.Processing.Parameters {
    internal class StringParam :ICloneable {
        public string Value { get; set; } = "";

        public object Clone() {
            return new StringParam { Value = Value };
        }
    }
}
