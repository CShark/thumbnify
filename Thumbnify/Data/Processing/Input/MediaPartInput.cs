using System.IO;
using FFmpeg.NET;
using FFmpeg.NET.Events;
using NAudio.Wave;
using Newtonsoft.Json;
using Thumbnify.Data.ParamStore;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Input {
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

        public Parameter<FlagParameter> CopyStream { get; } =
            new("copystream", false, new FlagParameter { Value = true });

        public MediaPartInput() {
            RegisterResult(Video);
            RegisterResult(Audio);
            RegisterResult(Name);

            RegisterParameter(CopyStream);
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
        public override string NodeTypeId => Id;

        public static string Id = "input_part";

        protected override bool Execute(CancellationToken cancelToken) {
            if (MediaPart == null) {
                Logger.Error("No part was defined");
                return false;
            }

            var path = TempPath;
            path = Path.Combine(path, Path.GetRandomFileName() + ".mp4");

            Logger.Debug("Cutting " + Path.GetFileName(MediaPart.Parent.FileName));
            Logger.Debug(
                $"Start Point: {TimeSpan.FromTicks(MediaPart.Start):g}, Duration: {TimeSpan.FromTicks(MediaPart.Duration):g}");

            // Cut Video
            var ffmpeg = new Engine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFmpeg\\ffmpeg.exe"));
            ffmpeg.Progress += FfmpegOnProgress;

            var opt = new ConversionOptions();
            if (CopyStream.Value.Value) {
                opt.ExtraArguments = "-c copy";
            }

            opt.CutMedia(new TimeSpan(MediaPart.Start), new TimeSpan(MediaPart.Duration));

            var input = new InputFile(MediaPart.Parent.FileName);
            var output = new OutputFile(path);


            if (cancelToken.IsCancellationRequested) return false;

            var task = ffmpeg.ConvertAsync(input, output, opt, cancelToken);
            Task.WaitAll(task);

            // Load Audio
            Logger.Debug("Convert Audio to mp3");
            ffmpeg.Progress -= FfmpegOnProgress;
            opt = new ConversionOptions();
            input = new InputFile(path);
            output = new OutputFile(Path.ChangeExtension(path, "mp3"));

            ffmpeg.ConvertAsync(input, output, opt, cancelToken).Wait(cancelToken);

            Audio.Value = new AudioStream {
                AudioFile = Path.ChangeExtension(path, "mp3")
            };

            Logger.Information("Audio converted to mp3");

            Video.Value = new VideoFile {
                VideoFileName = path
            };

            // Load Parameters
            foreach (var param in MediaPart.Metadata.Parameters) {
                var target = ParamStoreResults.FirstOrDefault(x => x.Id == param.Id);
                target?.SetValue(param.Value);
            }

            Name.SetValue(new StringParam {
                Value = MediaPart.Metadata.Name
            });

            return true;
        }

        private void FfmpegOnProgress(object? sender, ConversionProgressEventArgs e) {
            ReportProgress(e.ProcessedDuration.Ticks, MediaPart.Duration);
        }
    }
}