using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thumbnify.Data.Processing.Parameters;

namespace Thumbnify.Data.Processing {
    public abstract class Result {
        public string Id { get; set; }
        public Type Type { get; set; }

        public string? Name { get; set; }

        public abstract ParamType? GetValue();

        public abstract void SetValue(ParamType value);

        public abstract void Clear();

        public abstract void Dispose();

        protected Result(string id) {
            Id = id;
        }
    }

    public sealed class Result<T> : Result where T : ParamType {
        public T? Value { get; set; }

        public Result(string id) : base(id) {
            Type = typeof(T);
        }

        public override ParamType? GetValue() {
            return Value;
        }

        public override void SetValue(ParamType value) {
            if (value is T obj) {
                Value = obj;
            }
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