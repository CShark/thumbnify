using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;

namespace Thumbnify.Data.Processing.Parameters {
    internal class YoutubeVideoParam : ParamType {
        public Google.Apis.YouTube.v3.Data.Video Video { get; set; }

        public UserCredential Credentials { get; set; }

        public override ParamType Clone() {
            var json = JsonConvert.SerializeObject(Video);

            return new YoutubeVideoParam {
                Video = JsonConvert.DeserializeObject<Google.Apis.YouTube.v3.Data.Video>(json),
                Credentials = Credentials
            };
        }

        public override void Dispose() {
        }
    }
}