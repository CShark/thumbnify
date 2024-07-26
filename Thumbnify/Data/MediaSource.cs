using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thumbnify.Data {
    public class MediaSource : INotifyPropertyChanged {
        private bool _slatedForCleanup;
        public string Name { get; set; }
        public string FileName { get; set; }
        public DateTime Date { get; set; }

        [JsonIgnore]
        public bool SlatedForCleanup {
            get => _slatedForCleanup;
            set => SetField(ref _slatedForCleanup, value);
        }


        public ObservableCollection<MediaPart> Parts { get; set; } = new();

        [JsonIgnore]
        public bool FileExists { get; set; } = false;

        [JsonIgnore]
        public MediaUIData UiData { get; set; } = new();

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