using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tebisCloud.Data.Processing;

namespace tebisCloud.Data {
    public class ProcessingGraph {
        public List<Node> Nodes { get; set; } = new();

        public List<ProcessConnect> ProcessConnects { get; set; } = new();

        private HashSet<Node> _openNodes = new();

        private CancellationTokenSource _cancelToken;

        private Dictionary<string, Node> _nodes;
        private Dictionary<string, Dictionary<string, List<(string Node, string Port)>>> _edgesForward;
        private Dictionary<string, Dictionary<string, (string Node, string Result)?>> _edgesBackward;

        public ProcessingGraph() {
            //Nodes.Add(new PartSource());
        }

        public void RunGraph() {
            if (IsGraphRunning()) return;

            _cancelToken = new();
            _nodes = new();
            _edgesForward = new();
            _edgesBackward = new();

            // Clear Run Data
            foreach (var node in Nodes) {
                node.CancelToken = _cancelToken.Token;
                node.NodeCompleted -= OnNodeCompleted;
                node.ClearNode();

                _nodes[node.Id] = node;
                _edgesForward[node.Id] = new();
                _edgesBackward[node.Id] = new();

                foreach (var param in node.Parameters) {
                    _edgesBackward[node.Id][param.Key] = null;
                }

                foreach (var result in node.Results) {
                    _edgesForward[node.Id][result.Key] = new();
                }
            }

            foreach (var edge in ProcessConnects) {
                _edgesForward[edge.Previous][edge.PreviousPort].Add((edge.Next, edge.NextPort));
                _edgesBackward[edge.Next][edge.NextPort] = (edge.Previous, edge.PreviousPort);
            }

            // Build active graph to track overall progress
            var startNodes = Nodes.Where(x => _edgesBackward[x.Id].All(x => x.Value == null)).ToList();

            var openList = new List<Node>();
            foreach (var node in startNodes) {
                _openNodes.Add(node);

                openList.AddRange(GetChildren(node));

                while (openList.Any()) {
                    if (_openNodes.Add(openList[0])) {
                        openList.AddRange(GetChildren(openList[0]));
                    }

                    openList.RemoveAt(0);
                }
            }

            // Initialize Callbacks
            foreach (var node in _openNodes) {
                node.NodeCompleted += OnNodeCompleted;
            }

            // Set static values
            foreach (var node in Nodes.SelectMany(x =>
                         x.Parameters.Where(y => _edgesBackward[x.Id][y.Key] == null).Select(x => x.Value))) {
                node.ApplyDefaultValue();
            }

            // Run nodes without parameters
            foreach (var node in Nodes.Where(x => _edgesBackward[x.Id].Count == 0)) {
                node.TriggerNode();
            }

            IEnumerable<Node> GetChildren(Node node) {
                return _edgesForward[node.Id].SelectMany(x => x.Value).Select(x => _nodes[x.Node]).Distinct();
            }
        }

        public void CancelGraph() {
            _cancelToken.Cancel();
            _openNodes.Clear();
        }

        public bool IsGraphRunning() {
            return _openNodes.Any();
        }

        private void OnNodeCompleted(Node node) {
            _openNodes.Remove(node);
            node.NodeCompleted -= OnNodeCompleted;

            foreach (var result in node.Results) {
                var next = _edgesForward[node.Id][result.Key];

                if (next.Count == 0) {
                    result.Value.Dispose();
                    result.Value.Clear();
                } else {
                    for (int i = 1; i < next.Count; i++) {
                        var clone = result.Value.GetValue()?.Clone();
                        var param = _nodes[next[i].Node].Parameters[next[i].Port];

                        param.ApplyValue(clone);
                    }

                    _nodes[next[0].Node].Parameters[next[0].Port].ApplyValue(result.Value.GetValue());
                }
            }
        }
    }
}