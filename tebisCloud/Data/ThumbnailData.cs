using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using tebisCloud.Data.Thumbnail;

namespace tebisCloud.Data {
    public class ThumbnailData : INotifyPropertyChanged {
        private string _presetName;
        private ObservableCollection<ControlPart> _controls = new();
        private DateTime _created;

        public string PresetName {
            get => _presetName;
            set => SetField(ref _presetName, value);
        }

        public DateTime Created {
            get => _created;
            set => SetField(ref _created, value);
        }

        public ObservableCollection<ControlPart> Controls {
            get => _controls;
            set => SetField(ref _controls, value);
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