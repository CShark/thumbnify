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

namespace tebisCloud.Data.Processing {
    internal class AudioSaveFile : Node {
        public override IReadOnlyDictionary<string, Parameter> Parameters { get; }
        public override IReadOnlyDictionary<string, Result> Results { get; }
        public Parameter<AudioStream> AudioStream { get; set; } = new("audio", "Audio", true);
        public Parameter<FilePath> AudioFile { get; set; } = new("audio_file", "Dateiname", true, new(false,"MP3-Audio|*.mp3"));

        public AudioSaveFile() {
            Parameters = new Dictionary<string, Parameter>() {
                { AudioStream.Id, AudioStream },
                { AudioFile.Id, AudioFile }
            };

            Results = new Dictionary<string, Result>();

            InitializeParameters();
        }

        protected override bool Execute(CancellationToken cancelToken) {
            try {
                using (var writer = new LameMP3FileWriter(AudioFile.Value.FileName,
                           AudioStream.Value.WaveStream.WaveFormat, LAMEPreset.ABR_96)) {

                    var buffer = new byte[16 * 1024];

                    int read;
                    while ((read = AudioStream.Value.WaveStream.Read(buffer)) > 0) {
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

        public override EditorNode GenerateNode() {
            return new EditorNode("Audio Speichern", ENodeType.Audio, this);
        }
    }
}