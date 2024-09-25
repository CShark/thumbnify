using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace Thumbnify.Data {
    static class FileTools {
        public static void CopyStreams(Stream src, Stream dest, Action<double> progressCallback,
            CancellationToken token,
            uint bufferSize = 16 * 1024) {
            var length = src.Length;
            var buffer = new byte[bufferSize];
            int read = 0;

            while ((read = src.Read(buffer)) > 0) {
                if (token.IsCancellationRequested) return;

                dest.Write(buffer, 0, read);
                progressCallback((double)src.Position / src.Length);
            }
        }

        public static void CopySamples(ISampleProvider src, WaveFileWriter dst, Action progressCallback,
            CancellationToken token, ushort bufferSize = 16 * 1024) {
            var buffer = new float[bufferSize];
            int read = 0;

            while ((read = src.Read(buffer, 0, bufferSize)) > 0) {
                if (token.IsCancellationRequested) return;

                dst.WriteSamples(buffer, 0, read);
                progressCallback();
            }

            dst.Flush();
        }

        public static WaveFormat ToIEEE(this WaveFormat format) {
            return WaveFormat.CreateIeeeFloatWaveFormat(format.SampleRate, format.Channels);
        }

        public static string SanitizeFilename(string filename) {
            var parts = filename.Split('\\');

            foreach (var chr in Path.GetInvalidFileNameChars()) {
                for (var i = 1; i < parts.Length; i++) {
                    parts[i] = parts[i].Replace(chr.ToString(), "");
                }
            }

            foreach (var chr in Path.GetInvalidPathChars()) {
                for (var i = 1; i < parts.Length; i++) {
                    parts[i] = parts[i].Replace(chr.ToString(), "");
                }
            }

            return string.Join('\\', parts);
        }
    }
}