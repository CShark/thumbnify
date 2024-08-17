using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    public class StringParam : ParamType, INotifyPropertyChanged{
        private string _value = "";

        public string Value {
            get => _value;
            set => SetField(ref _value, value);
        }

        public bool ExtendedEditor { get; set; } = false;

        public StringParam() {

        }

        public StringParam(bool extended) {
            ExtendedEditor = extended;
        }

        public override ParamType Clone() {
            return new StringParam { Value = Value, ExtendedEditor = ExtendedEditor };
        }

        public override void Dispose() {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}