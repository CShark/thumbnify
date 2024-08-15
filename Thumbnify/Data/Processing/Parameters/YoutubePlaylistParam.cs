using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    public class YoutubePlaylistParam : ParamType {
        public YoutubeCredentialsParam Credentials { get; set; } = new();

        public string PlaylistId { get; set; }

        public override ParamType Clone() {
            return new YoutubePlaylistParam {
                Credentials = Credentials.Clone() as YoutubeCredentialsParam,
                PlaylistId = PlaylistId
            };
        }

        public override void Dispose() {
        }
    }
}