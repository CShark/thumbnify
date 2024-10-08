﻿using System;
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
                using (var dst = new FileStream(destFile, FileMode.Create))
                using (var mem = new MemoryStream()) {
                    var writer = new WaveFileWriter(dst, src.WaveFormat.ToIEEE());
                    var memory = new WaveFileWriter(mem, src.WaveFormat.ToIEEE());

                    var sampleReader = src.ToSampleProvider();
                    var lufsMeter = new R128LufsMeter();
                    lufsMeter.Prepare(src.WaveFormat.SampleRate, src.WaveFormat.Channels);

                    // calculate loudness
                    lufsMeter.StartIntegrated();
                    lufsMeter.ProcessBuffer(sampleReader, (position) => ReportProgress(src.Position, src.Length * 4),
                        CancelToken);
                    lufsMeter.StopIntegrated();

                    if (CancelToken.IsCancellationRequested) return false;
                    Logger.Debug($"Integrated Loudness: {lufsMeter.IntegratedLoudness} LU");

                    // apply gain
                    src.Seek(0, SeekOrigin.Begin);
                    var gaindB = (float)(targetLU - lufsMeter.IntegratedLoudness);
                    var gainLin = AudioCompressor.Db2Lin(gaindB);
                    Logger.Debug($"Apply gain of {gaindB}dB");

                    var buffer = new float[1024];
                    var read = 0;
                    var pos = 0;
                    while ((read = sampleReader.Read(buffer, 0, buffer.Length)) > 0) {
                        if (CancelToken.IsCancellationRequested) return false;

                        for (int i = 0; i < read; i++) {
                            buffer[i] *= gainLin;
                        }

                        memory.WriteSamples(buffer, 0, read);
                        ReportProgress(src.Position + src.Length, src.Length * 4);
                    }

                    memory.Flush();
                    mem.Seek(0, SeekOrigin.Begin);

                    // limit true peak
                    var memReader = new WaveFileReader(mem);
                    TruePeakLimiter.ProcessBuffer(memReader.ToSampleProvider(), writer, -1, src.WaveFormat.SampleRate,
                        .01, .3, 2, 2, x => { ReportProgress(memReader.Position + 2 * src.Length, src.Length * 4); });
                    writer.Flush();
                    dst.Seek(0, SeekOrigin.Begin);

                    // calculate final loudness
                    var dstReader = new WaveFileReader(dst);
                    lufsMeter.StartIntegrated();
                    lufsMeter.ProcessBuffer(dstReader.ToSampleProvider(),
                        (pos) => ReportProgress(dstReader.Position + 3 * src.Length, src.Length * 4),
                        CancelToken);
                    lufsMeter.StopIntegrated();

                    if (CancelToken.IsCancellationRequested) return false;
                    Logger.Debug($"Integrated Loudness after normalization: {lufsMeter.IntegratedLoudness} LU");
                }
            }

            AudioResult.Value = new AudioStream { AudioFile = destFile };

            return true;
        }
    }
}