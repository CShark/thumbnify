/*
 *    This file is a part of the LUFS metering utils
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
 *    Github : https://github.com/xuan525
 */

using System.IO;
using NAudio.Wave;

namespace Thumbnify.Data.Processing.Audio.R128 {
    /// <summary>
    /// ITU BS.1770 standard
    /// EN : https://www.itu.int/dms_pubrec/itu-r/rec/bs/R-REC-BS.1770-4-201510-I!!PDF-E.pdf
    /// CN : https://www.itu.int/dms_pubrec/itu-r/rec/bs/R-REC-BS.1770-4-201510-I!!PDF-C.pdf
    /// </summary>
    public class R128LufsMeter {
        // Store mean square infos of previous sample blocks from from the time the measurement was started (for integrated loudness)
        private struct MeanSquareLoudness {
            public double MeanSquare;
            public double Loudness;
        }

        private List<MeanSquareLoudness> PrecedingMeanSquareLoudness;

        // Store mean squares of previous sample blocks (for short-term loudness)
        private int ShortTermMeanSquaresLength;
        private double[] ShortTermMeanSquares;

        // Buffer for calc mean square for current sample block (for momentary loudness) double[step][channel][sample]
        private double[][] BlockSquaredSum { get; set; }
        private int NumChannel { get; set; }
        private int StepBufferPosition { get; set; }
        private double[] StepBuffer { get; set; }

        // Common
        private double BlockDuration { get; set; }
        private double OverLap { get; set; }
        private double StepDuration { get; set; }
        private double ShortTermDuration { get; set; }
        private int BlockSampleCount { get; set; }
        private int StepSampleCount { get; set; }
        private int BlockStepCount { get; set; }
        private SecondOrderIIRFilter PreFilter { get; set; }
        private SecondOrderIIRFilter HighPassFilter { get; set; }

        public delegate void LoudnessUpdatedHandler(double loudness);

        public LoudnessUpdatedHandler MomentaryLoudnessUpdated;
        public LoudnessUpdatedHandler ShortTermLoudnessUpdated;

        /// <summary>
        /// The consturctor of the R128LufsMeter
        /// </summary>
        public R128LufsMeter() : this(0.4, 0.75, 3) {
        }

        public R128LufsMeter(double blockDuration, double overlap, double shortTermDuration) {
            BlockDuration = blockDuration;
            OverLap = overlap;
            StepDuration = BlockDuration * (1 - OverLap);
            ShortTermDuration = shortTermDuration;
        }

        /// <summary>
        /// Prepare for process
        /// </summary>
        /// <param name="sampleRate">Sample rate of the sample data</param>
        /// <param name="numChannels">Number of channels of the sample data</param>
        public void Prepare(double sampleRate, int numChannels) {
            // init momentary loudness
            BlockSampleCount = (int)Math.Round(BlockDuration * sampleRate);
            StepSampleCount = (int)Math.Round(BlockSampleCount * (1 - OverLap));
            BlockStepCount = BlockSampleCount / StepSampleCount;

            NumChannel = numChannels;
            StepBufferPosition = 0;
            StepBuffer = new double[NumChannel];

            BlockSquaredSum = new double[BlockStepCount][];

            // init short-term loudness
            ShortTermMeanSquaresLength = (int)Math.Round(ShortTermDuration / StepDuration);
            ShortTermMeanSquares = new double[ShortTermMeanSquaresLength];

            // init integrated loudness
            IsIntegrating = false;

            PreFilter = new SecondOrderIIRFilter(
                1.53512485958697, // b0
                -2.69169618940638, // b1
                1.19839281085285, // b2
                -1.69065929318241, // a1
                0.73248077421585 // a2
            );
            PreFilter.Prepare(sampleRate, numChannels);

            HighPassFilter = new SecondOrderIIRFilter(
                1.0, // b0
                -2.0, // b1
                1.0, // b2
                -1.99004745483398, // a1
                0.99007225036621 // a2
            );
            HighPassFilter.Prepare(sampleRate, numChannels);
        }

        /// <summary>
        /// Models of realtime result returned by ProcessBuffer
        /// </summary>
        public class Result {
            public double MomentaryLoudness;
            public double ShortTermLoudness;
        }

        /// <summary>
        /// The integrated loudness started from the time when calling StartIntegrated
        /// </summary>
        public double IntegratedLoudness {
            get {
                if (PrecedingMeanSquareLoudness == null) {
                    return double.NaN;
                }

                // Gating of 400 ms blocks (overlapping by 75%), where two thresholds are used:
                //     The first at −70 LKFS;
                double absoluteGateGamma = -70;
                List<MeanSquareLoudness> absoluteGatedLoudness = new List<MeanSquareLoudness>();
                for (int i = 0; i < PrecedingMeanSquareLoudness.Count; i++) {
                    MeanSquareLoudness loudness = PrecedingMeanSquareLoudness[i];
                    if (loudness.Loudness > absoluteGateGamma) {
                        absoluteGatedLoudness.Add(loudness);
                    }
                }

                //      Calc relativeGateGamma
                double absoluteGatedMeanSquareSum = 0;
                for (int i = 0; i < absoluteGatedLoudness.Count; i++) {
                    MeanSquareLoudness loudness = absoluteGatedLoudness[i];
                    absoluteGatedMeanSquareSum += loudness.MeanSquare;
                }

                double absoluteGatedMeanSquare = absoluteGatedMeanSquareSum / absoluteGatedLoudness.Count;
                double relativeGateGamma = -0.691 + 10 * Math.Log10(absoluteGatedMeanSquare) - 10;


                //     The second at −10 dB relative to the level measured after application of the first threshold.
                List<MeanSquareLoudness> relativeGatedLoudness = new List<MeanSquareLoudness>();
                for (int i = 0; i < absoluteGatedLoudness.Count; i++) {
                    MeanSquareLoudness loudness = absoluteGatedLoudness[i];
                    if (loudness.Loudness > relativeGateGamma) {
                        relativeGatedLoudness.Add(loudness);
                    }
                }

                double relativeGatedMeanSquare = 0;
                double relativeGatedMeanSquareSum = 0;
                if (relativeGatedLoudness.Count > 0) {
                    for (int i = 0; i < relativeGatedLoudness.Count; i++) {
                        MeanSquareLoudness loudness = relativeGatedLoudness[i];
                        relativeGatedMeanSquareSum += loudness.MeanSquare;
                    }

                    relativeGatedMeanSquare = relativeGatedMeanSquareSum / relativeGatedLoudness.Count;
                }

                double integratedLoudness = -0.691 + 10 * Math.Log10(relativeGatedMeanSquare);
                return integratedLoudness;
            }
        }

        /// <summary>
        /// Describe the current mode of integrated loudness
        /// </summary>
        public bool IsIntegrating { get; set; }

        /// <summary>
        /// Start the measure of integrated loudness
        /// </summary>
        public void StartIntegrated() {
            ResetIntegrated();
            IsIntegrating = true;
        }

        /// <summary>
        /// Stop the measure of integrated loudness
        /// </summary>
        public void StopIntegrated() {
            IsIntegrating = false;
        }

        /// <summary>
        /// Reset the buffer for measuring integrated loudness
        /// </summary>
        public void ResetIntegrated() {
            PrecedingMeanSquareLoudness = new List<MeanSquareLoudness>();

            ShortTermMeanSquares = new double[ShortTermMeanSquaresLength];

            StepBufferPosition = 0;
            StepBuffer = new double[NumChannel];
        }

        /// <summary>
        /// Process a buffer of sample data
        /// </summary>
        /// <param name="buffer">The samples need to be process</param>
        /// <returns>A array of result with the interval of 100ms</returns>
        public void ProcessBuffer(ISampleProvider samples, Action<int> progressUpdated) {
            // Init the process
            int bufferPosition = 0;
            var tempBuffer = new float[NumChannel * StepSampleCount];
            var samplesRead = 0;

            while ((samplesRead = samples.Read(tempBuffer, 0, tempBuffer.Length)) > 0) {
                bufferPosition += samplesRead;
                progressUpdated?.Invoke(bufferPosition);

                StepBuffer = new double[NumChannel];
                for(int c=0; c<NumChannel; c++) {
                    StepBuffer[c] = 0;
                }
                
                // calculate squared sum for next step
                for (int i = 0; i < samplesRead; i++) {
                    // “K” frequency weighting
                    tempBuffer[i] = PreFilter.ProcessSample(tempBuffer[i], i % NumChannel);
                    tempBuffer[i] = HighPassFilter.ProcessSample(tempBuffer[i], i % NumChannel);

                    StepBuffer[i % NumChannel] += Math.Pow(tempBuffer[i], 2);
                }

                bufferPosition += StepSampleCount - StepBufferPosition;

                // Swap buffer
                double[] temp = BlockSquaredSum[0];
                for (int i = 1; i < BlockSquaredSum.Length; i++) {
                    BlockSquaredSum[i - 1] = BlockSquaredSum[i];
                }

                BlockSquaredSum[BlockSquaredSum.Length - 1] = StepBuffer;
                StepBuffer = temp;
                StepBufferPosition = 0;

                // Calc momentory loudness
                double momentaryMeanSquare = 0;
                if (BlockSquaredSum[0] != null) {
                    for (int channel = 0; channel < NumChannel; channel++) {
                        double channelSquaredSum = 0;

                        for (int step = 0; step < BlockStepCount; step++) {
                            channelSquaredSum += BlockSquaredSum[step][channel];
                        }

                        double channelMeanSquare = channelSquaredSum / (BlockStepCount * StepSampleCount);
                        double channelWeight = GetChannelWeight(channel);
                        momentaryMeanSquare += channelWeight * channelMeanSquare;
                    }
                } else {
                    momentaryMeanSquare = 0;
                }

                double momentaryLoudness = -0.691 + 10 * Math.Log10(momentaryMeanSquare);
                MomentaryLoudnessUpdated?.Invoke(momentaryLoudness);

                if (ShortTermLoudnessUpdated != null) {
                    // Calc short-term loudness
                    ShiftBuffer(ShortTermMeanSquares);
                    ShortTermMeanSquares[ShortTermMeanSquares.Length - 1] = momentaryMeanSquare;

                    double shortTermMeanSquaresSum = 0;
                    for (int i = 0; i < ShortTermMeanSquares.Length; i++) {
                        shortTermMeanSquaresSum += ShortTermMeanSquares[i];
                    }

                    double shortTermMeanSquareMean = shortTermMeanSquaresSum / ShortTermMeanSquares.Length;
                    double shortTermLoudness = -0.691 + 10 * Math.Log10(shortTermMeanSquareMean);
                    ShortTermLoudnessUpdated?.Invoke(shortTermLoudness);
                }

                // Calc integrated loudness
                if (IsIntegrating) {
                    MeanSquareLoudness meanSquareLoudness = new MeanSquareLoudness {
                        MeanSquare = momentaryMeanSquare,
                        Loudness = momentaryLoudness
                    };
                    PrecedingMeanSquareLoudness.Add(meanSquareLoudness);
                }
            }
        }


        private void ShiftBuffer(double[] buffer) {
            for (int i = 1; i < buffer.Length; i++) {
                buffer[i - 1] = buffer[i];
            }
        }

        public double GetChannelWeight(int channel) {
            double weight = 1;
            if (channel == 3 || channel == 4) {
                weight = 1.41;
            }

            return weight;
        }
    }
}