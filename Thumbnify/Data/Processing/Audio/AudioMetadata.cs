using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;

namespace Thumbnify.Data.Processing.Audio {
    public class AudioMetadata : Node {
        [JsonIgnore]
        public Parameter<AudioStream> AudioIn { get; } = new("audio", true);

        [JsonIgnore]
        public Result<AudioStream> AudioOut { get; } = new("audio");

        public Parameter<StringParam> Title { get; } = new("title", true, new());

        public Parameter<StringParam> Album { get; } = new("album", true, new());

        public Parameter<StringParam> Interpret { get; } = new("interpret", true, new());

        protected override ENodeType NodeType => ENodeType.Audio;

        public static string Id => "audio_metadata";

        public override string NodeTypeId => Id;

        public AudioMetadata() {
            RegisterParameter(AudioIn);
            RegisterParameter(Title);
            RegisterParameter(Album);
            RegisterParameter(Interpret);

            RegisterResult(AudioOut);
        }

        protected override bool Execute(CancellationToken cancelToken) {
            AudioOut.Value = AudioIn.Value.Clone() as AudioStream;

            AudioOut.Value.Title = Title.Value?.Value;
            AudioOut.Value.Album = Album.Value?.Value;
            AudioOut.Value.Interpret = Interpret.Value?.Value;

            return true;
        }
    }
}