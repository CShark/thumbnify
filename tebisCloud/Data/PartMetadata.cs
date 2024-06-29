using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data {
    public class PartMetadata :INotifyPropertyChanged{
        private string _prediger = "";
        private string _thema = "";
        private string? _title;
        private string? _fileName;
        private DateTime _date;

        public string Prediger {
            get => _prediger;
            set {
                SetField(ref _prediger, value);
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(FileName));
            }
        }

        public string Thema {
            get => _thema;
            set {
                SetField(ref _thema, value);
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(FileName));
            }
        }

        public string Title {
            get => _title ?? $"{Prediger} - {Thema}";
            set {
                if (!string.IsNullOrWhiteSpace(_title)) {
                    SetField(ref _title, value);
                } else {
                    SetField(ref _title, null);
                }
            }
        }

        public string FileName {
            get => _fileName ?? $"{Prediger} - {Thema}";
            set {
                if (!string.IsNullOrWhiteSpace(value)) {
                    SetField(ref _fileName, value);
                } else {
                    SetField(ref _fileName, value);
                }
            }
        }

        public DateTime Date {
            get => _date;
            set => SetField(ref _date, value);
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
