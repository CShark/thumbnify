using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyleafLib.MediaPlayer;
using NAudio.Wave;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Audio {
    internal sealed class AudioLoadFile : Node {
        public Parameter<FilePath> AudioFile { get; } = new("path", true,
            new(FilePath.EPathMode.OpenFile, "Alle Dateien|*.mp4;*.mkv;*.mp3"));

        [JsonIgnore]
        public Result<AudioStream> AudioStream { get; } = new("audio");


        public AudioLoadFile() {
            RegisterParameter(AudioFile);
            RegisterResult(AudioStream);
        }

        protected override ENodeType NodeType => ENodeType.Audio;
        public override string NodeTypeId => Id;
        public static string Id => "audio_load";

        protected override bool Execute(CancellationToken cancelToken) {
            if (!File.Exists(AudioFile.Value.FileName)) {
                Logger.Error("Audio file does not exist");
                return false;
            }

            var file = Path.Combine(TempPath, Path.GetRandomFileName() + ".wav");

            using (var src = new AudioFileReader(AudioFile.Value.FileName)) {
                using (var dst = new WaveFileWriter(file, src.WaveFormat.ToIEEE())) {
                    FileTools.CopySamples(src, dst,
                        () => ReportProgress(src.Position, src.Length), CancelToken);
                }
            }

            AudioStream.Value = new AudioStream {
                AudioFile = file
            };

            return true;
        }
    }
}