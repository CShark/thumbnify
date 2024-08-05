using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    class DateParam : ParamType {
        public DateTime Value { get; set; } = DateTime.Today;

        public bool Today { get; set; }

        public DateTime ResolveDate() {
            if (Today) {
                return DateTime.Now;
            } else {
                return Value;
            }
        }

        public override ParamType Clone() {
            return new DateParam {
                Value = Value,
                Today = Today
            };
        }

        public override void Dispose() {
        }
    }
}