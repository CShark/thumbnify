using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using NAudio.Wave;
using tebisCloud.Data.Processing.Parameters;
using tebisCloud.Postprocessing;

namespace tebisCloud.Data.Processing {
    internal class AudioSaveFile : Node {
        public override IReadOnlyDictionary<string, Parameter> Parameters { get; }
        public override IReadOnlyDictionary<string, Result> Results { get; }

        public Parameter<AudioStream> AudioStream { get; set; } = new("audio", "Audio", true);

        public Parameter<FilePath> AudioFile { get; set; } = new("audio_file", "Dateiname", true, new());

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
                WaveFileWriter.CreateWaveFile(AudioFile.Value.FileName, AudioStream.Value.WaveStream);
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