using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data.Processing {
    public abstract class Result {
        public string Id { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }

        public abstract ICloneable? GetValue();

        public abstract void Clear();

        public abstract void Dispose();

        protected Result(string id, string name) {
            Id = id;
            Name = name;
        }
    }

    public sealed class Result<T> : Result where T : ICloneable {
        public T? Value { get; set; }

        public Result(string id, string name) : base(id, name) {
            Type = typeof(T);
        }

        public override ICloneable? GetValue() {
            return Value;
        }

        public override void Clear() {
            Value = default;
        }

        public override void Dispose() {
            if (Value is IDisposable d) {
                d.Dispose();
            }
        }
    }
}