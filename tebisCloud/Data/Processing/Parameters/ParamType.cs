using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace tebisCloud.Data.Processing.Parameters {
    [JsonConverter(typeof(JsonKnownTypesConverter<ParamType>))]
    public abstract class ParamType : IDisposable {
        public abstract ParamType Clone();
        public abstract void Dispose();
    }
}