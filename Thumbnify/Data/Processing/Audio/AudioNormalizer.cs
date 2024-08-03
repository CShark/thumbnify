using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.NET.Enums;
using NAudio.Wave;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Audio.R128;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Audio {
    class AudioNormalizer : Node {
        [JsonIgnore]
        public Parameter<AudioStream> AudioInput { get; } = new("audio", true);

        [JsonIgnore]
        public Result<AudioStream> AudioResult { get; } = new("audio");

        public Parameter<IntParam> TargetLoudness { get; } = new("target_lu", true, new IntParam { Value = -14 });

        protected override ENodeType NodeType => ENodeType.Audio;
        public override string NodeTypeId => Id;

        public static string Id { get; } = "audio_normalize";

        public AudioNormalizer() {
            RegisterParameter(AudioInput);
            RegisterParameter(TargetLoudness);

            RegisterResult(AudioResult);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var targetLU = TargetLoudness.Value.Value;
            var destFile = Path.Combine(TempPath, Path.GetRandomFileName() + ".wav");

            using (var src = AudioInput.Value.GetWaveStream()) {
                using (var dst = new FileStream(destFile, FileMode.Create)) {
                    var writer = new WaveFileWriter(dst, src.WaveFormat);

                    var sampleReader = src.ToSampleProvider();
                    var lufsMeter = new R128LufsMeter();
                    lufsMeter.Prepare(src.WaveFormat.SampleRate, src.WaveFormat.Channels);

                    // calculate loudness
                    lufsMeter.StartIntegrated();
                    lufsMeter.ProcessBuffer(sampleReader, (position) => ReportProgress(position, src.Length * 3));
                    lufsMeter.StopIntegrated();

                    Logger.Debug($"Integrated Loudness: {lufsMeter.IntegratedLoudness} LU");

                    // apply gain
                    src.Seek(0, SeekOrigin.Begin);
                    var gaindB = (float)(targetLU - lufsMeter.IntegratedLoudness);
                    var gainLin = AudioCompressor.Db2Lin(gaindB);
                    Logger.Debug($"Apply gain of {gaindB}dB");

                    var buffer = new float[1024];
                    var read = 0;
                    while ((read = sampleReader.Read(buffer, 0, buffer.Length)) > 0) {
                        for (int i = 0; i < read; i++) {
                            buffer[i] *= gainLin;
                        }

                        writer.WriteSamples(buffer, 0, read);
                        ReportProgress(src.Position + src.Length, src.Length);
                    }

                    dst.Seek(0, SeekOrigin.Begin);

                    // calculate final loudness
                    var dstReader = new WaveFileReader(dst);
                    lufsMeter.StartIntegrated();
                    lufsMeter.ProcessBuffer(dstReader.ToSampleProvider(),
                        (pos) => ReportProgress(pos + 2 * dst.Length, dst.Length * 3));
                    lufsMeter.StopIntegrated();

                    Logger.Debug($"Integrated Loudness after normalization: {lufsMeter.IntegratedLoudness} LU");
                }
            }

            return true;
        }
    }
}