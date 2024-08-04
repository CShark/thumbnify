using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal class AudioMerge : Node {
        [JsonIgnore]
        public Result<AudioStream> AudioResult { get; } = new("audio");

        [JsonIgnore]
        public Parameter<AudioStream> Audio1 { get; } = new("audio1", true);

        [JsonIgnore]
        public Parameter<AudioStream> Audio2 { get; } = new("audio2", true);

        protected override ENodeType NodeType => ENodeType.Audio;

        public static string Id => "audio_merge";

        public override string NodeTypeId => Id;

        public AudioMerge() {
            RegisterParameter(Audio1);
            RegisterParameter(Audio2);

            RegisterResult(AudioResult);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            using (var src1 = Audio1.Value.GetWaveStream())
            using (var src2 = Audio2.Value.GetWaveStream()) {
                if (src1.WaveFormat.Channels != src2.WaveFormat.Channels ||
                    src1.WaveFormat.SampleRate != src2.WaveFormat.SampleRate ||
                    src1.WaveFormat.BitsPerSample != src2.WaveFormat.BitsPerSample) {
                    Logger.Error(
                        $"Both items need to have the same number of channels ({src1.WaveFormat.Channels}, {src2.WaveFormat.Channels}), sample rate ({src1.WaveFormat.SampleRate}, {src2.WaveFormat.SampleRate}) and bit depth ({src1.WaveFormat.BitsPerSample}, {src2.WaveFormat.BitsPerSample})");

                    return false;
                }

                var concatProvider =
                    new ConcatenatingSampleProvider(new[] { src1.ToSampleProvider(), src2.ToSampleProvider() });

                var path = Path.Combine(TempPath, Path.GetRandomFileName() + ".wav");

                using (var dst = new WaveFileWriter(path, src1.WaveFormat)) {
                    FileTools.CopySamples(concatProvider, dst,
                        () => ReportProgress(src1.Position + src2.Position, src1.Length + src2.Length), CancelToken);
                }

                AudioResult.Value = new AudioStream { AudioFile = path };
            }

            return true;
        }
    }
}