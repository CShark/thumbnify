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

        protected override ENodeType NodeType => ENodeType.Audio;
        public static string Id => "audio_resample";
        public override string NodeTypeId => Id;

        public AudioResample() {
            RegisterParameter(AudioParam);
            RegisterParameter(Samples);

            RegisterResult(AudioResult);
        }


        protected override bool Execute(CancellationToken cancelToken) {
            if (!int.TryParse(Samples.Value.Value, out var sampleRate)) {
                Logger.Error($"Could not parse sample rate {Samples.Value.Value}");
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

                var targetFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, src.WaveFormat.Channels);

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