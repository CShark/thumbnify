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
        private ThumbnailData _thumbnail;
        private bool _edited;

        public ThumbnailData Thumbnail {
            get => _thumbnail;
            set => SetField(ref _thumbnail, value);
        }

        public bool Edited {
            get => _edited;
            set => SetField(ref _edited, value);
        }

        public override ParamType Clone() {
            return new ThumbnailParam {
                Thumbnail = Thumbnail,
                Edited = Edited
            };
        }

        public override void Dispose() {
        }

        public BitmapSource RenderThumbnail(ObservableCollection<ParamDefinition>? paramList) {
            RenderTargetBitmap render = null;

            App.Current.Dispatcher.Invoke(() => {
                var json = JsonConvert.SerializeObject(Thumbnail);
                var copy = JsonConvert.DeserializeObject<ThumbnailData>(json);

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