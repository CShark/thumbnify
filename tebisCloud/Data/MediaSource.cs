using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thumbnify.Data {
    public class MediaSource {
        public string Name { get; set; }
        public string FileName { get; set; }
        public DateTime Date { get; set; }

        public ObservableCollection<MediaPart> Parts { get; set; } = new();

        [JsonIgnore]
        public bool FileExists { get; set; } = false;

        [JsonIgnore]
        public MediaUIData UiData { get; set; } = new();
    }
}
