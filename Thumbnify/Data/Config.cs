using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Thumbnify.Data.ParamStore;

namespace Thumbnify.Data {
    public class Config : INotifyPropertyChanged {
        private string _videoPath = "";
        private ObservableCollection<MediaSource> _media = new();
        private ObservableCollection<ThumbnailData> _thumbnails = new();
        private string _defaultThumbnail;
        private ObservableCollection<ProcessingGraph> _processing = new();
        private string _defaultProcessing;
        private ObservableCollection<YoutubeCredentials> _youtubeCredentials = new();

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

        public ObservableCollection<ProcessingGraph> Processing {
            get => _processing;
            set => SetField(ref _processing, value);
        }

        public string DefaultProcessing {
            get => _defaultProcessing;
            set => SetField(ref _defaultProcessing, value);
        }

        public ObservableCollection<YoutubeCredentials> YoutubeCredentials {
            get => _youtubeCredentials;
            set => SetField(ref _youtubeCredentials, value);
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