using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using Microsoft.VisualBasic.FileIO;
using NAudio.Wave;
using Newtonsoft.Json;
using tebisCloud.Data.ParamStore;
using tebisCloud.Data.Processing.Parameters;
using tebisCloud.Postprocessing;

namespace tebisCloud.Data.Processing.Input {
    public sealed class MediaPartInput : Node {
        [JsonIgnore]
        public Result<VideoFile> Video { get; } = new("video");

        [JsonIgnore]
        public Result<AudioStream> Audio { get; } = new("audio");

        [JsonIgnore]
        public Result<StringParam> Name { get; } = new("name");

        [JsonIgnore]
        public MediaPart? MediaPart { get; set; }

        [JsonIgnore]
        public List<Result> ParamStoreResults { get; } = new();
        
        public MediaPartInput() {
            RegisterResult(Video);
            RegisterResult(Audio);
            RegisterResult(Name);
        }

        public void SetParamStore(IList<ParamDefinition> store) {
            ClearResults();

            RegisterResult(Video);
            RegisterResult(Audio);
            RegisterResult(Name);

            ParamStoreResults.Clear();

            foreach (var param in store) {
                var res = param.BuildResult();

                if (res != null) {
                    ParamStoreResults.Add(res);
                    RegisterResult(res);
                }
            }

            OnPortsChanged();
        }

        protected override ENodeType NodeType => ENodeType.Parameter;
        protected override string NodeId => Id;

        public static string Id = "input_part";

        protected override bool Execute(CancellationToken cancelToken) {
            if (MediaPart == null) {
                LogMessage("No part was defined");
                return false;
            }

            var path = MediaPart.GetTempDir();
            path = Path.Combine(path, Path.GetRandomFileName() + ".mp4");

            // Cut Video
            var ffmpeg = new Engine("");
            ffmpeg.Progress += FfmpegOnProgress;

            var opt = new ConversionOptions();
            opt.CutMedia(new TimeSpan(MediaPart.Start), new TimeSpan(MediaPart.Duration));

            var input = new InputFile(MediaPart.Parent.FileName);
            var output = new OutputFile(path);


            if (cancelToken.IsCancellationRequested) return false;
            
            var task = ffmpeg.ConvertAsync(input, output, opt, cancelToken);
            Task.WaitAll(task);
            
            // Load Audio
            Audio.Value = new AudioStream {
                WaveStream = new MediaFoundationReader(path)
            };

            Video.Value = new VideoFile {
                VideoFileName = path
            };

            // Load Parameters
            foreach (var param in MediaPart.Metadata.Parameters) {
                var target = ParamStoreResults.FirstOrDefault(x => x.Id == param.Id);
                target?.SetValue(param.Value);
            }

            return true;
        }

        private void FfmpegOnProgress(object? sender, ConversionProgressEventArgs e) {
            ReportProgress(e.ProcessedDuration.Ticks, e.TotalDuration.Ticks);
        }
    }
}