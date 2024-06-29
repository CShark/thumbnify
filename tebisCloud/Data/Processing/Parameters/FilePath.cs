using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data.Processing.Parameters {
    public class FilePath : ICloneable {
        public string FileName { get; set; }

        public object Clone() {
            return new FilePath {
                FileName = FileName
            };
        }
    }
}