using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Thumbnify.Data.Processing.Parameters {
    public class AudioStream : ParamType {
        public WaveStream WaveStream { get; set; }

        public override ParamType Clone() {
            var mem = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(mem, WaveStream);
            mem.Seek(0, SeekOrigin.Begin);
            WaveStream.Seek(0, SeekOrigin.Begin);
            return new AudioStream {WaveStream = new WaveFileReader(mem)};
        }

        public override void Dispose() {
            WaveStream.Dispose();
        }
    }
}