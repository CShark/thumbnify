using System.IO;
using NAudio.Lame;
using NAudio.Wave;
using Newtonsoft.Json;
using TagLib;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;
using File = System.IO.File;

namespace Thumbnify.Data.Processing.Audio {
    internal sealed class AudioSaveFile : Node {
        [JsonIgnore]
        public Parameter<AudioStream> AudioStream { get; } = new("audio", true);

        public Parameter<FilePath> AudioFile { get; } = new("path", true,
            new(FilePath.EPathMode.SaveFile, "MP3-Audio|*.mp3"));

        [JsonIgnore]
        public Result<FilePath> AudioResult { get; } = new("path");

        public AudioSaveFile() {
            RegisterParameter(AudioStream);
            RegisterParameter(AudioFile);

            RegisterResult(AudioResult);
        }

        protected override ENodeType NodeType => ENodeType.Audio;
        public override string NodeTypeId => Id;
        public static string Id => "audio_save";

        protected override bool Execute(CancellationToken cancelToken) {
            var filename = AudioFile.Value.FileName;

            if (!filename.ToLower().EndsWith(".mp3")) {
                filename += ".mp3";
            }

            filename = FileTools.SanitizeFilename(filename);

            var dir = Path.GetDirectoryName(filename);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(filename)) {
                File.Delete(filename);
            }

            try {
                using (var src = AudioStream.Value.GetWaveStream()) {
                    using (var writer = new LameMP3FileWriter(filename,
                               src.WaveFormat, LAMEPreset.ABR_96)) {
                        FileTools.CopyStreams(src, writer, ReportProgress, CancelToken);
                    }
                }

                // Write Metadata
                var tagFile = TagLib.File.Create(filename);
                if (!string.IsNullOrWhiteSpace(AudioStream.Value.Title)) {
                    tagFile.Tag.Title = AudioStream.Value.Title;
                }

                if (!string.IsNullOrWhiteSpace(AudioStream.Value.Album)) {
                    tagFile.Tag.Album = AudioStream.Value.Album;
                }

                if (!string.IsNullOrWhiteSpace(AudioStream.Value.Interpret)) {
                    tagFile.Tag.Composers = new[] { AudioStream.Value.Interpret };
                }

                tagFile.RemoveTags(TagTypes.Id3v1);
                tagFile.Save();

                AudioResult.Value = new FilePath(filename);

                return true;
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to save audio");
                return false;
            }
        }
    }
}