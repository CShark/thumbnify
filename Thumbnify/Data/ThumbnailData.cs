using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Thumbnify.Data.Thumbnail;

namespace Thumbnify.Data {
    public class ThumbnailData : INotifyPropertyChanged, IDialogItem {
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

        public string? Base64Image { get; set; }

        public string Name => PresetName;

        [JsonIgnore]
        public BitmapSource? Preview { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            if (Base64Image != null) {
                var bytes = Convert.FromBase64String(Base64Image);
                using (var ms = new MemoryStream(bytes)) {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();

                    Preview = bmp;
                }
            }
        }

        [OnSerializing]
        internal void OnSerializing(StreamingContext context) {
            if (Preview != null) {
                var encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 70;
                encoder.Frames.Add(BitmapFrame.Create(Preview));

                using (var ms = new MemoryStream()) {
                    encoder.Save(ms);

                    var bytes = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(bytes);

                    Base64Image = Convert.ToBase64String(bytes);
                }
            }
        }
    }
}