using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace tebisCloud.Data.Processing.Parameters {
    public class FilePath : ICloneable, INotifyPropertyChanged {

        public enum EPathMode {
            SaveFile,
            OpenFile,
            Directory
        }

        private string _fileName;

        public string FileName {
            get => _fileName;
            set => SetField(ref _fileName, value);
        }

        [JsonIgnore]
        public EPathMode Mode { get; }

        [JsonIgnore]
        public string Filter { get; }

        public object Clone() {
            return new FilePath {
                FileName = FileName
            };
        }

        private FilePath() {
        }

        public FilePath(EPathMode mode, string filter) {
            Mode = mode;
            Filter = filter;
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