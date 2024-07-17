using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using NAudio.Lame;
using NAudio.Wave;
using tebisCloud.Data.Processing.Parameters;
using tebisCloud.Postprocessing;

namespace tebisCloud.Data.Processing.Audio {
    internal sealed class AudioSaveFile : Node {
        public Parameter<AudioStream> AudioStream { get; } = new("audio", true);

        public Parameter<FilePath> AudioFile { get; } = new("path", true,
            new(FilePath.EPathMode.SaveFile, "MP3-Audio|*.mp3"));

        public AudioSaveFile() {
            RegisterParameter(AudioStream);
            RegisterParameter(AudioFile);
        }

        protected override ENodeType NodeType => ENodeType.Audio;
        protected override string NodeId => Id;
        public static string Id => "audio_save";

        protected override bool Execute(CancellationToken cancelToken) {
            throw new Exception("Test");

            try {
                using (var writer = new LameMP3FileWriter(AudioFile.Value.FileName,
                           AudioStream.Value.WaveStream.WaveFormat, LAMEPreset.ABR_96)) {
                    var buffer = new byte[16 * 1024];

                    int read;
                    while ((read = AudioStream.Value.WaveStream.Read(buffer)) > 0) {
                        if (cancelToken.IsCancellationRequested) return false;
                        writer.Write(buffer);
                        ReportProgress(AudioStream.Value.WaveStream.Position, AudioStream.Value.WaveStream.Length);
                    }
                }

                return true;
            } catch (Exception ex) {
                LogMessage("Failed to save audio: " + ex);
                return false;
            }
        }
    }
}