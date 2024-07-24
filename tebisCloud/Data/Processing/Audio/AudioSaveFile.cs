using System.IO;
using NAudio.Lame;
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
            var filename = AudioFile.Value.FileName;

            if (!filename.ToLower().EndsWith(".mp3")) {
                filename += ".mp3";
            }

            if (File.Exists(filename)) {
                File.Delete(filename);
            }

            try {
                using (var writer = new LameMP3FileWriter(filename,
                           AudioStream.Value.WaveStream.WaveFormat, LAMEPreset.ABR_96)) {
                    FileTools.CopyStreams(AudioStream.Value.WaveStream, writer, ReportProgress, CancelToken);
                }

                return true;
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to save audio");
                return false;
            }
        }
    }
}