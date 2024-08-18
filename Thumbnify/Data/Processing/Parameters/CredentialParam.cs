using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    class CredentialParam : ParamType {
        public string Username { get; set; }

        public string Password { get; set; }

        public NetworkCredential BuildCredentials() {
            return new NetworkCredential(Username, Password);
        }
        
        public override ParamType Clone() {
            return new CredentialParam {
                Username = Username, Password = Password
            };
        }

        public override void Dispose() {
        }
    }
}