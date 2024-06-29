using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data {
    public class Config : INotifyPropertyChanged {
        private string _videoPath = "";
        private ObservableCollection<MediaSource> _media = new();
        private ObservableCollection<ThumbnailData> _thumbnails = new();
        private string _defaultThumbnail;

        public string VideoPath {
            get => _videoPath;
            set => SetField(ref _videoPath, value);
        }

        public ObservableCollection<MediaSource> Media {
            get => _media;
            set => SetField(ref _media, value);
        }

        public ObservableCollection<ThumbnailData> Thumbnails {
            get => _thumbnails;
            set => SetField(ref _thumbnails, value);
        }

        public string DefaultThumbnail {
            get => _defaultThumbnail;
            set => SetField(ref _defaultThumbnail, value);
        }

        public ThumbnailData? GetDefaultThumbnail() {
            return Thumbnails.FirstOrDefault(x => x.PresetName.ToLower() == DefaultThumbnail.ToLower());
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