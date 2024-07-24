using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thumbnify.NAudio;

namespace Thumbnify.Data {
    public class MediaUIData {
        public Stream? FileStream { get; set; }

        public NAudioEngine? AudioEngine { get; set; }
    }
}
