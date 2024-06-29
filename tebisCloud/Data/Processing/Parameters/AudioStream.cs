using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace tebisCloud.Data.Processing.Parameters {
    internal class AudioStream : ICloneable, IDisposable {
        public WaveStream WaveStream { get; set; }

        public object Clone() {
            var mem = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(mem, WaveStream);
            mem.Seek(0, SeekOrigin.Begin);
            return new WaveFileReader(mem);
        }

        public void Dispose() {
            WaveStream.Dispose();
        }
    }
}