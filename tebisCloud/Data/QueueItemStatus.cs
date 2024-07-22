using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data {
    public class QueueItemStatus {
        public QueueItemStatus(MediaPart mediaPart) {
            MediaPart = mediaPart;
        }

        public MediaPart MediaPart { get; set; }

    }
}