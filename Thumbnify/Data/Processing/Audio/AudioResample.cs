using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Audio {
    internal class AudioResample : Node {
        [JsonIgnore]
        public Parameter<AudioStream> AudioParam { get; } = new("audio", true);

        [JsonIgnore]
        public Result<AudioStream> AudioResult { get; } = new("audio");

        public Parameter<EnumParameter> Samples { get; } =
            new("samples", false, new("44100", new Dictionary<string, string> {
                { "44.100", "44100" },
                { "48.000", "48000" },
                { "96.000", "96000" }
            }));

        public Parameter<EnumParameter> BitDepth { get; } = new("bitdepth", false, new("16",
            new Dictionary<string, string> {
                { "16bit", "16" },
                { "24bit", "24" },
                { "32bit", "32" }
            }));

        protected override ENodeType NodeType => ENodeType.Audio;
        public static string Id => "audio_resample";
        public override string NodeTypeId => Id;

        public AudioResample() {
            RegisterParameter(AudioParam);
            RegisterParameter(Samples);
            RegisterParameter(BitDepth);

            RegisterResult(AudioResult);
        }


        protected override bool Execute(CancellationToken cancelToken) {
            if (!int.TryParse(Samples.Value.Value, out var sampleRate)) {
                Logger.Error($"Could not parse sample rate {Samples.Value.Value}");
                return false;
            }

            if (!int.TryParse(BitDepth.Value.Value, out var bitdepth)) {
                Logger.Error($"Could not parse bit depth {BitDepth.Value.Value}");
                return false;
            }

            using (var src = AudioParam.Value.GetWaveStream()) {
                if (src.WaveFormat.SampleRate == sampleRate) {
                    Logger.Information("No resampling needed");
                    AudioResult.SetValue(AudioParam.Value.Clone());
                    return true;
                }

                Logger.Debug($"Resampling from {src.WaveFormat.SampleRate} to {sampleRate}");

                var resampler = new WdlResamplingSampleProvider(src.ToSampleProvider(), sampleRate);
                var path = Path.Combine(TempPath, Path.GetRandomFileName() + ".wav");

                var targetFormat = new WaveFormat(sampleRate, bitdepth, src.WaveFormat.Channels);

                if (bitdepth == 32) {
                    targetFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, src.WaveFormat.Channels);
                }

                using (var writer =
                       new WaveFileWriter(path, targetFormat)) {
                    FileTools.CopySamples(resampler, writer, () => ReportProgress(src.Position, src.Length),
                        cancelToken);
                }

                AudioResult.Value = new AudioStream { AudioFile = path };
            }

            return true;
        }
    }
}