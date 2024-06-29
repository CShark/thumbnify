using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace tebisCloud.Data.Thumbnail {
    [JsonConverter(typeof(JsonKnownTypesConverter<ControlPart>))]
    public abstract class ControlPart : INotifyPropertyChanged {
        private string _name;
        private double _top;
        private double _left;
        private double _height;
        private double _width;
        private bool _isSelected;

        public double Width {
            get => _width;
            set => SetField(ref _width, value);
        }

        public double Height {
            get => _height;
            set => SetField(ref _height, value);
        }

        public double Left {
            get => _left;
            set => SetField(ref _left, value);
        }

        public double Top {
            get => _top;
            set => SetField(ref _top, value);
        }

        public string Name {
            get => _name;
            set => SetField(ref _name, value);
        }

        [JsonIgnore]
        public bool IsSelected {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        [JsonIgnore]
        public abstract bool FormatingSupport { get; }


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