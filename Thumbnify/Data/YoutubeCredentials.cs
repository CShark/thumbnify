using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace Thumbnify.Data {
    public class YoutubeCredentials : IDialogItem {
        public static string BasePath => Path.Combine(App.AppDirectory, "youtube.auth");

        public string Guid { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public BitmapSource? Preview { get; private set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            var thumb = Path.Combine(BasePath, Guid, "thumbnail.png");

            if (File.Exists(thumb)) {
                var img = new BitmapImage();
                img.BeginInit();
                img.UriSource = new Uri(thumb);
                img.EndInit();
                Preview = img;
            }
        }
    }
}