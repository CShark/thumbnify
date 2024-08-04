﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thumbnify.Data {
    public class QueueItemStatus {
        public QueueItemStatus(MediaPart mediaPart, bool refetchGraph = true) {
            MediaPart = mediaPart;

            var graph = mediaPart.Metadata.ProcessingGraph;
            var orig = App.Settings.Processing.FirstOrDefault(x => x.Name.ToLower() == graph.Name.ToLower());

            if (orig != null) {
                graph = orig;
            }
            
            var json = JsonConvert.SerializeObject(graph);
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