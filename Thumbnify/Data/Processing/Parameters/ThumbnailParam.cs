using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Thumbnify.Controls;
using Thumbnify.Data.ParamStore;

namespace Thumbnify.Data.Processing.Parameters {
    public class ThumbnailParam : ParamType, INotifyPropertyChanged {
        private bool _edited;
        private string? _thumbnailPreset;
        private ThumbnailData? _thumbnail;

        public ThumbnailData? GetThumbnail() {
            var orig =  _thumbnail ?? App.Settings.Thumbnails.FirstOrDefault(x => x.PresetName == _thumbnailPreset);
            var json = JsonConvert.SerializeObject(orig);
            return JsonConvert.DeserializeObject<ThumbnailData>(json);
        }

        public bool Edited => _thumbnail != null;

        public string? ThumbnailPreset {
            get => _thumbnailPreset;
            set {
                if (value == _thumbnailPreset) return;
                _thumbnailPreset = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Thumbnail));
            }
        }

        public ThumbnailData? LocalThumbnail {
            get => _thumbnail;
            set {
                if (Equals(value, _thumbnail)) return;
                _thumbnail = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Thumbnail));
            }
        }

        [JsonIgnore]
        public ThumbnailData Thumbnail => GetThumbnail();

        public override ParamType Clone() {
            return new ThumbnailParam {
                _thumbnail = _thumbnail,
                _thumbnailPreset = _thumbnailPreset
            };
        }

        public override void Dispose() {
        }

        public BitmapSource RenderThumbnail(ObservableCollection<ParamDefinition>? paramList) {
            RenderTargetBitmap render = null;

            App.Current.Dispatcher.Invoke(() => {
                var copy = GetThumbnail();

                var preview = new ThumbnailPreview();
                preview.Thumbnail = copy;
                preview.Measure(new Size(1920, 1080));
                preview.Arrange(new Rect(0, 0, 1920, 1080));
                preview.SetParameters(paramList);
                preview.UpdateLayout();

                render = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
                render.Render(preview);
            });

            return render;
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