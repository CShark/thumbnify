using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tebisCloud.Data.Processing.Parameters;
using WPFLocalizeExtension.Engine;

namespace tebisCloud.Data.Processing {
    public abstract class Parameter {
        public event Action ValueChanged;

        public string Id { get; set; }

        [JsonIgnore]
        public bool Bindable { get; set; }

        [JsonIgnore]
        public bool ShowName { get; set; }

        [JsonIgnore]
        public abstract Type Type { get; }

        [JsonIgnore]
        public abstract bool HasValue { get; }

        [JsonIgnore]
        public abstract ParamType? DefaultValueObj { get; }

        protected Parameter(string id, bool bindable, bool showName) {
            Id = id;
            Bindable = bindable;
            ShowName = showName;
        }

        public abstract void ApplyDefaultValue();

        public abstract void ApplyValue(ParamType? value);

        public abstract void Clear();

        protected virtual void OnValueChanged() {
            ValueChanged?.Invoke();
        }
    }

    public sealed class Parameter<T> : Parameter where T : ParamType {
        private T? _value = default;

        public override Type Type => typeof(T);

        [JsonIgnore]
        public T? Value {
            get => _value;
            set {
                _value = value;
                OnValueChanged();
            }
        }

        public T? DefaultValue { get; set; }

        public override bool HasValue => Value != null;

        public override ParamType? DefaultValueObj => DefaultValue;

        public Parameter(string id, bool bindable, T? defaultValue = default, bool showName = true) : base(
            id, bindable, showName) {
            DefaultValue = defaultValue;
        }

        public override void ApplyDefaultValue() {
            Value = DefaultValue;
        }

        public override void ApplyValue(ParamType? value) {
            if (value is T t) {
                Value = t;
            } else {
                Value = default;
            }
        }

        public override void Clear() {
            _value?.Dispose();
            _value = default;
        }
    }
}