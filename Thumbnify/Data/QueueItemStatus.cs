using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thumbnify.Data {
    public class QueueItemStatus {
        public QueueItemStatus(MediaPart mediaPart) {
            MediaPart = mediaPart;

            var json = JsonConvert.SerializeObject(MediaPart.Metadata.ProcessingGraph);
            Graph = JsonConvert.DeserializeObject<ProcessingGraph>(json);
        }

        public QueueItemStatus(ProcessingGraph graph) {
            var json = JsonConvert.SerializeObject(graph);
            Graph = JsonConvert.DeserializeObject<ProcessingGraph>(json);
        }

        public MediaPart? MediaPart { get; }

        public ProcessingGraph Graph { get; }
    }
}