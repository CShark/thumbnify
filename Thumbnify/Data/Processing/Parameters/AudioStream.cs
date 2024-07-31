using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Thumbnify.Data.Processing.Parameters {
    public class AudioStream : ParamType {
        public string AudioFile { get; set; }

        public AudioFileReader GetWaveStream() {
            return new AudioFileReader(AudioFile);
        }

        public override ParamType Clone() {
            return new AudioStream { AudioFile = AudioFile };
        }

        public override void Dispose() {
        }
    }
}