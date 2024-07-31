using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Thumbnify.Data {
    public class MediaPart : INotifyPropertyChanged {
        private long _start;
        private long _end;
        private string _name;
        private long _duration;
        private string _title;
        private ThumbnailData _thumbnail;
        private PartMetadata _metadata = new();
        private bool _processingCompleted;
        private DateTime? _processingCompletedDate;

        public long Start {
            get => _start;
            set => SetField(ref _start, value);
        }

        public long End {
            get => _end;
            set => SetField(ref _end, value);
        }

        public string Name {
            get => _name;
            set => SetField(ref _name, value);
        }

        public PartMetadata Metadata {
            get => _metadata;
            set => SetField(ref _metadata, value);
        }
        
        public long Duration {
            get => _duration;
            set => SetField(ref _duration, value);
        }

        public bool ProcessingCompleted {
            get => _processingCompleted;
            set => SetField(ref _processingCompleted, value);
        }

        public DateTime? ProcessingCompletedDate {
            get => _processingCompletedDate;
            set => SetField(ref _processingCompletedDate, value);
        }


        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore]
        public MediaSource Parent { get; set; }

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