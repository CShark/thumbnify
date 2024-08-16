using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Thumbnify.Data.Processing.Parameters {
    public class YoutubePlaylistParam : ParamType ,INotifyPropertyChanged{
        private string _playlistId;
        public YoutubeCredentialsParam Credentials { get; set; } = new();

        public string PlaylistId {
            get => _playlistId;
            set => SetField(ref _playlistId, value);
        }

        public override ParamType Clone() {
            return new YoutubePlaylistParam {
                Credentials = Credentials.Clone() as YoutubeCredentialsParam,
                PlaylistId = PlaylistId
            };
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