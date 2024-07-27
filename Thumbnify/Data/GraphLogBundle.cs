using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Thumbnify.Data.Processing;
using Thumbnify.Tools;

namespace Thumbnify.Data {
    class GraphLogBundle {
        public ProcessingGraph Graph { get; set; }

        public LocalLogSink LogMessages { get; set; }

        public List<NodeState> NodeStates { get; set; } = new();

        public double GraphProgress { get; set; }
        public ENodeStatus GraphState { get; set; }

        public GraphLogBundle() {
        }

        public GraphLogBundle(ProcessingGraph graph) {
            Graph = graph;
            LogMessages = graph.LogMessages;

            NodeStates = graph.Nodes.Select(x => new NodeState {
                NodeUid = x.Uid,
                Progress = x.Progress,
                State = x.NodeStatus,
            }).ToList();

            GraphProgress = graph.Progress;
            GraphState = graph.GraphState;
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            if (Graph != null && LogMessages != null) {
                Graph.LogMessages = LogMessages;
                Graph.Progress = GraphProgress;
                Graph.GraphState = GraphState;

                foreach (var node in NodeStates) {
                    var match = Graph.Nodes.FirstOrDefault(x => x.Uid == node.NodeUid);

                    if (match != null) {
                        match.Progress = node.Progress;
                        match.NodeStatus = node.State;
                    }
                }
            }
        }

        public class NodeState {
            public string NodeUid { get; set; }
            public double Progress { get; set; }
            public ENodeStatus State { get; set; }
        }
    }
}