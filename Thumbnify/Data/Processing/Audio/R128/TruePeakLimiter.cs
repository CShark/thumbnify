/*
 *    This file is a part of the TruePeakLimiter utils
 *    Copyright (C) 2020  Xuan525
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 *
 *    Email : shanboxuan@me.com
 *    Github : https://github.com/xuan25/R128Normalization
 */


using NAudio.Wave;

namespace Thumbnify.Data.Processing.Audio.R128 {
    public class TruePeakLimiter {
        public double LinearThreashold { get; set; }
        public int AttackSample { get; set; }
        public int ReleaseSample { get; set; }
        public int DelaySample => AttackSample;
        public double AttackCurveTension { get; set; }
        public double ReleaseCurveTension { get; set; }

        private double CurrentValue { get; set; }
        private double AttackStartValue { get; set; }
        private double AttackEndValue { get; set; }
        private int AttackPosition { get; set; }
        private double ReleaseStartValue { get; set; }
        private int ReleasePosition { get; set; }

        /// <summary>
        /// Constructor of TruePeakLimiter
        /// </summary>
        /// <param name="threasholdDb">Threshold in dB</param>
        /// <param name="sampleRate">Sample rate of the samples</param>
        /// <param name="attack">Attack duration in seconds</param>
        /// <param name="release">Release duration in seconds</param>
        /// <param name="attackCurve">Attack curve tension</param>
        /// <param name="releaseCurve">Release curve tension</param>
        public TruePeakLimiter(double threasholdDb, double sampleRate, double attack, double release,
            double attackCurve, double releaseCurve) {
            LinearThreashold = Math.Pow(10, threasholdDb / 20);
            AttackSample = (int)Math.Round(sampleRate * attack);
            ReleaseSample = (int)Math.Round(sampleRate * release);
            AttackCurveTension = attackCurve;
            ReleaseCurveTension = releaseCurve;

            // init attack and release status
            CurrentValue = 1;
            AttackStartValue = -1;
            AttackEndValue = -1;
            AttackPosition = -1;
            ReleaseStartValue = -1;
            ReleasePosition = -1;
        }

        /// <summary>
        /// Process a whole buffer of samples
        /// </summary>
        /// <param name="buffer">The buffer need to be processed</param>
        /// <param name="threshold">Threshold of the limiter in dB</param>
        /// <param name="sampleRate">Sample rate of the samples</param>
        /// <param name="attack">Attack duration of the limiter in seconds</param>
        /// <param name="release">Release duration of the limiter in seconds</param>
        /// <param name="attackCurve">Attack curve tension of the limiter</param>
        /// <param name="releaseCurve">Release curve tension of the limiter</param>
        /// <param name="progressUpdated">ProgressUpdated event handler</param>
        public static void ProcessBuffer(ISampleProvider samples, WaveFileWriter sampleWriter, double threshold,
            double sampleRate, double attack, double release, double attackCurve, double releaseCurve,
            Action<int> progressUpdated) {
            TruePeakMeter[] truePeakMeters = new TruePeakMeter[samples.WaveFormat.Channels];
            for (int i = 0; i < truePeakMeters.Length; i++) {
                truePeakMeters[i] = new TruePeakMeter();
            }

            TruePeakLimiter truePeakLimiter =
                new TruePeakLimiter(threshold, sampleRate, attack, release, attackCurve, releaseCurve);

            var bufferSize = Math.Max(1024, truePeakLimiter.DelaySample);
            var tempBuffer = new float[samples.WaveFormat.Channels * 1024];
            var samplesRead = 0;
            var bufferPosition = 0;

            var queue = new Queue<float>[samples.WaveFormat.Channels];
            for (var i = 0; i < queue.Length; i++) {
                queue[i] = new();
            }

            // Prefill queue
            samplesRead = samples.Read(tempBuffer, 0, truePeakLimiter.DelaySample * samples.WaveFormat.Channels);
            bufferPosition += samplesRead;
            progressUpdated?.Invoke(bufferPosition);
            for (var i = 0; i < samplesRead; i++) {
                queue[i % samples.WaveFormat.Channels].Enqueue(tempBuffer[i]);
            }

            // Process buffer one channel pair at a time
            while ((samplesRead = samples.Read(tempBuffer, 0, tempBuffer.Length)) > 0) {
                bufferPosition += samplesRead;
                progressUpdated?.Invoke(bufferPosition);

                for (var i = 0; i < samplesRead; i += samples.WaveFormat.Channels) {
                    var peak = 0d;

                    for (var c = 0; c < samples.WaveFormat.Channels; c++) {
                        var sample = tempBuffer[i + c];
                        var channelPeak = truePeakMeters[c].NextTruePeak(sample);
                        if (channelPeak > peak) {
                            peak = channelPeak;
                        }

                        queue[c].Enqueue(sample);
                    }

                    var ratio = truePeakLimiter.ProcessNext(peak);
                    for (var c = 0; c < samples.WaveFormat.Channels; c++) {
                        sampleWriter.WriteSample((float)(queue[c].Dequeue() * ratio));
                    }
                }
            }

            // Process remaining queue
            while (queue[0].Count > 0) {
                var ratio = truePeakLimiter.ProcessNext(0);

                for (var c = 0; c < samples.WaveFormat.Channels; c++) {
                    sampleWriter.WriteSample((float)(queue[c].Dequeue() * ratio));
                }
            }
        }

        /// <summary>
        /// Process the next sample
        /// </summary>
        /// <param name="peak">the ture peak of the sample</param>
        /// <returns>The envelope ratio of the sample with a delay of DelaySample</returns>
        public double ProcessNext(double peak) {
            if (AttackPosition != -1 && AttackPosition < AttackSample) {
                AttackPosition++;
                double p = AttackCurveFunction((double)AttackPosition / AttackSample);
                double value = AttackStartValue - p * (AttackStartValue - AttackEndValue);
                CurrentValue = value;
            } else if (ReleasePosition != -1 && ReleasePosition < ReleaseSample) {
                ReleasePosition++;
                double p = ReleaseCurveFunction((double)ReleasePosition / ReleaseSample);
                double value = ReleaseStartValue + p * (1 - ReleaseStartValue);
                CurrentValue = value;
            } else {
                CurrentValue = 1;
            }

            if (peak * CurrentValue > LinearThreashold) {
                AttackStartValue = CurrentValue;
                AttackEndValue = LinearThreashold / peak;
                AttackPosition = 0;
                ReleaseStartValue = AttackEndValue;
                ReleasePosition = 0;
            }

            return CurrentValue;
        }

        private double ReleaseCurveFunction(double x) {
            return Math.Pow(x, ReleaseCurveTension);
        }

        private double AttackCurveFunction(double x) {
            return 1 - Math.Pow(1 - x, AttackCurveTension);
        }
    }
}