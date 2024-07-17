using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonKnownTypes;

namespace tebisCloud.Data.Processing.Parameters {
    [JsonConverter(typeof(JsonKnownTypesConverter<ParamType>))]
    public abstract class ParamType : IDisposable {
        public abstract ParamType Clone();
        public abstract void Dispose();
    }
}