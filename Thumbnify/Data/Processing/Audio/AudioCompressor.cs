using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NAudio.Wave;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Audio {
    class AudioCompressor : Node {
        // Source: https://github.com/velipso/sndfilter/blob/master/src/compressor.c
        private const int MaxDelay = 1024;
        private const int SPU = 32;
        private const float SpacingDb = 5f;

        public static RoutedUICommand OpenParameters { get; } = new();

        [JsonIgnore]
        public Parameter<AudioStream> AudioParameter { get; } = new("audio", true);

        [JsonIgnore]
        public Result<AudioStream> AudioResult { get; } = new("audio");

        public Parameter<CompressorParam> CompressorParameters { get; } = new("parameters", false, new(), false);

        protected override ENodeType NodeType => ENodeType.Audio;

        public static string Id => "audio_compressor";

        public override string NodeTypeId => Id;

        public AudioCompressor() {
            RegisterParameter(AudioParameter);
            RegisterParameter(CompressorParameters);

            RegisterResult(AudioResult);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            var source = AudioParameter.Value.GetWaveStream();
            var destFile = Path.Combine(TempPath, Path.GetRandomFileName() + ".wav");
            var output = new WaveFileWriter(destFile, source.WaveFormat);
            var state = InitializeState(source.WaveFormat.SampleRate);

            var channels = source.WaveFormat.Channels;
            var delayBuffer = new float[state.DelayBufSize * channels];

            var samplesPerChunk = SPU;
            var chunks = source.Length / samplesPerChunk;
            var ang90 = Math.PI * .5;
            var ang90inv = 2 / Math.PI;
            var samplePos = 0;
            var spacingDb = SpacingDb;

            for (var ch = 0; ch < chunks; ch++) {
                ReportProgress(ch, chunks);

                state.DetectorAvg = Fix(state.DetectorAvg, 1);
                var desiredGain = state.DetectorAvg;
                var scaledDesiredGain = (float)(Math.Asin(desiredGain) * ang90inv);
                var compDiffDb = Lin2Db(state.CompGain / scaledDesiredGain);

                // calculate envelope rate based on whether we're attacking or releasing
                float enveloperate;
                if (compDiffDb < 0.0f) {
                    compDiffDb = Fix(compDiffDb, -1.0f);
                    state.MaxCompDiffDb = -1;

                    float x = (Math.Clamp(compDiffDb, -12.0f, 0.0f) + 12.0f) * 0.25f;
                    float releasesamples = AdaptiveReleaseCurve(x, state.A, state.B, state.C, state.D);
                    enveloperate = Db2Lin(SpacingDb / releasesamples);
                } else {
                    compDiffDb = Fix(compDiffDb, 1.0f);
                    if (state.MaxCompDiffDb == -1 || state.MaxCompDiffDb < compDiffDb)
                        state.MaxCompDiffDb = compDiffDb;
                    float attenuate = state.MaxCompDiffDb;
                    if (attenuate < 0.5f)
                        attenuate = 0.5f;
                    enveloperate = (float)(1.0f - Math.Pow(0.25f / attenuate, state.AttackSamplesInv));
                }

                // process the chunk
                for (int chi = 0; chi < samplesPerChunk; chi++) {
                    var input = new float[samplesPerChunk * channels];
                    source.Read(input, 0, input.Length);

                    state.DelayReadPos = (state.DelayReadPos + 1) % state.DelayBufSize;
                    state.DelayWritePos = (state.DelayWritePos + 1) % state.DelayBufSize;

                    var sample = new float[channels];
                    for (int i = 0; i < channels; i++) {
                        sample[i] = input[chi * source.WaveFormat.Channels + i] * state.LinearPreGain;
                        delayBuffer[state.DelayWritePos * channels + i] = sample[i];
                        sample[i] = Math.Abs(sample[i]);
                    }


                    var inputMax = sample.Max();
                    var attenuation = 1f;

                    if (inputMax >= 0.0001) {
                        var inputComp = CompCurve(inputMax, state.K, state.Slope, state.LinearThreshold,
                            state.LinearThresholdKnee, state.Threshold, state.Knee, state.KneeBOffset);
                        attenuation = inputComp / inputMax;
                    }


                    var rate = 1f;
                    if (attenuation > state.DetectorAvg) {
                        var attenuationDb = -Lin2Db(attenuation);
                        if (attenuationDb < 2) {
                            attenuationDb = 2;
                        }

                        var dbPerSample = attenuationDb * state.SatReleaseSamplesInv;
                        rate = Db2Lin(dbPerSample) - 1;
                    }


                    state.DetectorAvg += (attenuation - state.DetectorAvg) * rate;
                    if (state.DetectorAvg > 1) {
                        state.DetectorAvg = 1;
                    }

                    state.DetectorAvg = Fix(state.DetectorAvg, 1);

                    if (enveloperate < 1) {
                        state.CompGain += (scaledDesiredGain - state.CompGain) * enveloperate;
                    } else {
                        state.CompGain *= enveloperate;

                        if (state.CompGain > 1) {
                            state.CompGain = 1;
                        }
                    }

                    // final gain value
                    var premixGain = (float)Math.Sin(ang90 * state.CompGain);
                    var gain = state.Dry + state.Wet * state.MasterGain * premixGain;

                    var premixGainDb = Lin2Db(premixGain);
                    if (premixGainDb < state.MeterGain) {
                        state.MeterGain = premixGainDb;
                    } else {
                        state.MeterGain += (premixGainDb - state.MeterGain) * state.MeterRelease;
                    }

                    for (int i = 0; i < channels; i++) {
                        output.WriteSample(sample[i] * gain);
                    }
                }
            }

            output.Flush();
            output.Close();

            AudioResult.Value = new AudioStream { AudioFile = destFile };

            return true;
        }

        private CompressorState InitializeState(int sampleRate) {
            var state = new CompressorState();
            var config = CompressorParameters.Value;

            state.DelayBufSize = (int)(sampleRate * config.PreDelay);
            if (state.DelayBufSize < 1) {
                state.DelayBufSize = 1;
            } else if (state.DelayBufSize > MaxDelay) {
                state.DelayBufSize = MaxDelay;
            }

            state.LinearPreGain = Db2Lin(config.PreGain);
            state.LinearThreshold = Db2Lin(config.Threshold);
            state.Slope = 1 / config.Ratio;
            state.AttackSamplesInv = 1 / (sampleRate * config.Attack / 1000);
            state.SatReleaseSamplesInv = 1 / (sampleRate * 0.0025f);
            state.Wet = config.Wet / 100;
            state.Dry = 1 - config.Wet / 100;
            state.Threshold = config.Threshold;
            float releaseSamples = sampleRate * config.Release / 1000;

            state.MeterGain = 1;
            state.MeterRelease = (float)(1 - Math.Exp(-1 / (sampleRate * 0.325f)));

            state.K = 5;
            state.KneeBOffset = 0;
            state.LinearThresholdKnee = 0;
            state.Knee = config.Knee;
            if (config.Knee > 0) {
                var xknee = Db2Lin(state.Threshold + state.Knee);
                var mink = 0.1f;
                var maxk = 10000f;

                for (int i = 0; i < 15; i++) {
                    if (KneeSlope(xknee, state.K, state.LinearThreshold) < state.Slope) {
                        maxk = state.K;
                    } else {
                        mink = state.K;
                    }

                    state.K = (float)Math.Sqrt(mink * maxk);
                }

                state.KneeBOffset = Lin2Db(KneeCurve(xknee, state.K, state.LinearThreshold));
                state.LinearThresholdKnee = Db2Lin(state.Threshold + state.Knee);
            }


            var fullLevel = CompCurve(1, state.K, state.Slope, state.LinearThreshold, state.LinearThresholdKnee,
                state.Threshold, state.Knee, state.KneeBOffset);
            state.MasterGain = (float)(Db2Lin(config.PostGain) * Math.Pow(1 / fullLevel, 0.6));

            var y1 = releaseSamples * config.ReleaseZone1;
            var y2 = releaseSamples * config.ReleaseZone2;
            var y3 = releaseSamples * config.ReleaseZone3;
            var y4 = releaseSamples * config.ReleaseZone4;
            state.A = (-y1 + 3 * y2 - 3 * y3 + y4) / 6;
            state.B = y1 - 2.5f * y2 + 2 * y3 - .5f * y4;
            state.C = (-11 * y1 + 18 * y2 - 9 * y3 + 2 * y4) / 6;
            state.D = y1;

            state.DetectorAvg = 0;
            state.CompGain = 1;
            state.MaxCompDiffDb = -1;
            state.DelayWritePos = 0;
            state.DelayReadPos = state.DelayBufSize > 1 ? 1 : 0;

            return state;
        }

        public static float KneeSlope(float x, float k, float linearThreshold) {
            return (float)(k * x / ((k * linearThreshold + 1.0f) * Math.Exp(k * (x - linearThreshold)) - 1));
        }

        public static float KneeCurve(float x, float k, float linearThreshold) {
            return (float)(linearThreshold + (1 - Math.Exp(-k * (x - linearThreshold))) / k);
        }

        public static float AdaptiveReleaseCurve(float x, float a, float b, float c, float d) {
            float x2 = x * x;
            return a * x2 * x + b * x2 + c * x + d;
        }

        public static float CompCurve(float x, float k, float slope, float linearThreshold, float linearThresholdKnee,
            float threshold, float knee, float kneeBOffset) {
            if (x < linearThreshold) {
                return x;
            }

            if (knee <= 0.0) {
                return Db2Lin(threshold + slope * (Lin2Db(x) - threshold));
            }

            if (x < linearThresholdKnee) {
                return KneeCurve(x, k, linearThreshold);
            }

            return Db2Lin(kneeBOffset + slope * (Lin2Db(x) - threshold - knee));
        }

        public static float Db2Lin(float db) {
            return (float)Math.Pow(10.0, 0.05 * db);
        }

        public static float Lin2Db(float lin) {
            return (float)(20 * Math.Log10(lin));
        }

        public static float Fix(float v, float def) {
            if (float.IsNaN(v) || float.IsInfinity(v)) {
                return def;
            }

            return v;
        }

        private struct CompressorState {
            public float MeterGain;
            public float MeterRelease;
            public float Threshold;
            public float Knee;
            public float LinearPreGain;
            public float LinearThreshold;
            public float Slope;
            public float AttackSamplesInv;
            public float SatReleaseSamplesInv;
            public float Wet;
            public float Dry;
            public float K;
            public float KneeBOffset;
            public float LinearThresholdKnee;
            public float MasterGain;
            public float A, B, C, D;
            public float DetectorAvg;
            public float CompGain;
            public float MaxCompDiffDb;

            public int DelayBufSize;
            public int DelayWritePos;
            public int DelayReadPos;
        }
    }
}