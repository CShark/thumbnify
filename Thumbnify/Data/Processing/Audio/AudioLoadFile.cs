using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyleafLib.MediaPlayer;
using NAudio.Wave;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Audio {
    internal sealed class AudioLoadFile : Node {
        public Parameter<FilePath> AudioFile { get; } = new("path", true,
            new(FilePath.EPathMode.OpenFile, "Alle Dateien|*.mp4;*.mkv;*.mp3"));

        [JsonIgnore]
        public Result<AudioStream> AudioStream { get; } = new("audio");


        public AudioLoadFile() {
            RegisterParameter(AudioFile);
            RegisterResult(AudioStream);
        }

        protected override ENodeType NodeType => ENodeType.Audio;
        public override string NodeTypeId => Id;
        public static string Id => "audio_load";

        protected override bool Execute(CancellationToken cancelToken) {
            try {
                if (!File.Exists(AudioFile.Value.FileName)) {
                    Logger.Error("Audio file does not exist");
                    return false;
                }

                AudioStream.Value = new AudioStream {
                    WaveStream = new AudioFileReader(AudioFile.Value.FileName)
                };

                return true;
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to load audio file");
                return false;
            }
        }


        //public async Task<EStepStatus> ExecuteInternal(CancellationToken cancelToken) {
        //    //if (!File.Exists(AudioFile)) {
        //    //    MessageLog = "Source file was not found";
        //    //    return EStepStatus.Error;
        //    //} else {
        //    //    try {
        //    //        var source = new AudioFileReader(AudioFile);

        //    //        if (Successors["audio"].Count > 1) {
        //    //            for (int i = 1; i < Successors["audio"].Count; i++) {
        //    //                if (cancelToken.IsCancellationRequested) return EStepStatus.Cancelled;

        //    //                var mem = new MemoryStream();
        //    //                source.Seek(0, SeekOrigin.Begin);
        //    //                WaveFileWriter.WriteWavFileToStream(mem, source);
        //    //                var audio = new WaveFileReader(mem);

        //    //                if (Successors["audio"][i].step.ProvideValue(Successors["audio"][i].port, audio)) {
        //    //                    var localStep = Successors["audio"][i].step;
        //    //                    Task.Run(async () => await localStep.Execute(cancelToken), cancelToken);
        //    //                }
        //    //            }
        //    //        }

        //    //        if (Successors["audio"].Count > 0) {
        //    //            if (cancelToken.IsCancellationRequested) return EStepStatus.Cancelled;

        //    //            if (Successors["audio"][0].step.ProvideValue(Successors["audio"][0].port, source)) {
        //    //                Task.Run(async () => await Successors["audio"][0].step.Execute(cancelToken), cancelToken);
        //    //            }
        //    //        }

        //    //        return EStepStatus.Completed;
        //    //    } catch (Exception ex) {
        //    //        MessageLog = "Error loading file:\n" + ex;
        //    //        return EStepStatus.Error;
        //    //    }
        //    //}
        //}
    }
}