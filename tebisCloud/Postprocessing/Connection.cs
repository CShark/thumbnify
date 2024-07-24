using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Thumbnify.Postprocessing {
    public class Connection {
        public Connector Source { get; }
        public Connector Target { get; }

        public Type Type { get; }
        
        public Connection(Connector source, Connector target, Type type) {
            Source = source;
            Target = target;
            Type = type;
        }
    }
}
