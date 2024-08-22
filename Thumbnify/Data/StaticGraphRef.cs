using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data {
    public class StaticGraphRef {
        public StaticGraphRef(string graphName) {
            GraphName = graphName;
        }

        public string GraphName { get; set; }
    }
}
