﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace tebisCloud.Data.Processing {
    public abstract class Parameter {
        public event Action ValueChanged;

        public string Id { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public bool Bindable { get; set; }

        [JsonIgnore]
        public bool ShowName { get; set; }

        [JsonIgnore]
        public abstract Type Type { get; }

        [JsonIgnore]
        public abstract bool HasValue { get; }

        [JsonIgnore]
        public abstract object? DefaultValueObj { get; }

        protected Parameter(string id, string name, bool bindable, bool showName) {
            Id = id;
            Name = name;
            Bindable = bindable;
            ShowName = showName;
        }

        public abstract void ApplyDefaultValue();

        public abstract void ApplyValue(object? value);

        public abstract void Clear();

        protected virtual void OnValueChanged() {
            ValueChanged?.Invoke();
        }
    }

    public sealed class Parameter<T> : Parameter where T : ICloneable {
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

        public override object? DefaultValueObj => DefaultValue;

        public Parameter(string id, string name, bool bindable, T? defaultValue = default, bool showName = true) : base(
            id, name, bindable, showName) {
            DefaultValue = defaultValue;
        }

        public override void ApplyDefaultValue() {
            Value = DefaultValue;
        }

        public override void ApplyValue(object? value) {
            if (value is T t) {
                Value = t;
            } else {
                Value = default;
            }
        }

        public override void Clear() {
            if (_value is IDisposable d) {
                d.Dispose();
            }

            _value = default;
        }
    }
}