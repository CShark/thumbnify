using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Serilog;
using Thumbnify.Data.ParamStore;
using Thumbnify.Data.Processing;
using Thumbnify.Data.Processing.Input;
using Thumbnify.Tools;

namespace Thumbnify.Data {
    public class ProcessingGraph : IDialogItem, INotifyPropertyChanged {
        public ObservableCollection<Node> Nodes { get; } = new();

        public ObservableCollection<ProcessConnect> ProcessConnects { get; set; } = new();

        public ObservableCollection<ParamDefinition> Parameters {
            get => _parameters;
            set => SetField(ref _parameters, value);
        }

        private HashSet<Node> _openNodes = new();

        private CancellationTokenSource _cancelToken;

        private Dictionary<string, Node> _nodes;
        private Dictionary<string, Dictionary<string, List<(string Node, string Port)>>> _edgesForward;
        private Dictionary<string, Dictionary<string, (string Node, string Result)?>> _edgesBackward;
        private string _name;
        private ObservableCollection<ParamDefinition> _parameters = new();
        private double _progress;
        private ENodeStatus _graphState;

        private ILogger _logger;
        private LocalLogSink _logMessages;
        private MediaPart? _media;

        [JsonIgnore]
        public double Progress {
            get => _progress;
            set => SetField(ref _progress, value);
        }

        [JsonIgnore]
        public ENodeStatus GraphState {
            get => _graphState;
            set => SetField(ref _graphState, value);
        }

        [JsonIgnore]
        public LocalLogSink LogMessages {
            get => _logMessages;
            set => SetField(ref _logMessages, value);
        }

        [JsonIgnore]
        public bool RequiresMediaPart => Nodes.Any(x => x is MediaPartInput);

        public ProcessingGraph() {
            Nodes.CollectionChanged += (_, args) => {
                if (args.NewItems != null) {
                    foreach (Node node in args.NewItems) {
                        if (node is MediaPartInput mediaPart) {
                            mediaPart.SetParamStore(Parameters);
                        }
                    }
                }
            };

            Parameters.CollectionChanged += (_, _) => {
                foreach (var node in Nodes.OfType<MediaPartInput>()) {
                    node.SetParamStore(Parameters);
                }
            };
        }

        public void AddNode(Node node) {
            if (!IsGraphRunning()) {
                Nodes.Add(node);
            }
        }

        public void RemoveNode(Node node) {
            if (!IsGraphRunning()) {
                Nodes.Remove(node);
                var items = ProcessConnects.Where(x => x.Next == node.Uid || x.Previous == node.Uid).ToList();

                foreach (var item in items) {
                    ProcessConnects.Remove(item);
                }
            }
        }

        public void RunGraph(MediaPart part) {
            _media = part;

            foreach (var node in Nodes) {
                if (node is MediaPartInput mediaNode) {
                    mediaNode.MediaPart = part;
                }
            }

            RunGraph();
        }

        public void RunGraph() {
            if (IsGraphRunning()) return;

            TempPath = Path.Combine(App.AppDirectory, Path.GetRandomFileName());
            if (!Directory.Exists(TempPath)) {
                Directory.CreateDirectory(TempPath);
            }

            LogMessages = new();
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(LogMessages)
                .CreateLogger();
            _logger.Information("Starting graph");

            _cancelToken = new();
            _nodes = new();
            _edgesForward = new();
            _edgesBackward = new();
            GraphState = ENodeStatus.Running;

            Progress = 0;

            _logger.Information("Initializing Nodes");
            // Clear Run Data
            foreach (var node in Nodes) {
                node.CancelToken = _cancelToken.Token;
                node.NodeCompleted -= OnNodeCompleted;
                node.ResolveParameters -= OnNodeResolveParameters;
                node.PropertyChanged -= NodeOnPropertyChanged;
                node.ClearNode();

                node.TempPath = TempPath;

                _nodes[node.Uid] = node;
                _edgesForward[node.Uid] = new();
                _edgesBackward[node.Uid] = new();

                foreach (var param in node.Parameters) {
                    _edgesBackward[node.Uid][param.Key] = null;
                }

                foreach (var result in node.Results) {
                    _edgesForward[node.Uid][result.Key] = new();
                }

                node.PropertyChanged += NodeOnPropertyChanged;
                node.ResolveParameters += OnNodeResolveParameters;
                node.SetLogger(_logger.ForContext(new NodeEnricher(node)));
            }

            foreach (var edge in ProcessConnects) {
                _edgesForward[edge.Previous][edge.PreviousPort].Add((edge.Next, edge.NextPort));
                _edgesBackward[edge.Next][edge.NextPort] = (edge.Previous, edge.PreviousPort);
            }

            // Build active graph to track overall progress
            var startNodes = Nodes.Where(x => _edgesBackward[x.Uid].All(x => x.Value == null)).ToList();

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
                         x.Parameters.Where(y => _edgesBackward[x.Uid][y.Key] == null).Select(x => x.Value))) {
                node.ApplyDefaultValue();
            }

            // Run nodes without parameters
            _logger.Information("Starting Initial Nodes");
            foreach (var node in Nodes.Where(x => _edgesBackward[x.Uid].Count == 0)) {
                node.TriggerNode();
            }

            IEnumerable<Node> GetChildren(Node node) {
                return _edgesForward[node.Uid].SelectMany(x => x.Value).Select(x => _nodes[x.Node]).Distinct();
            }
        }

        public void CancelGraph() {
            _cancelToken.Cancel();
            _openNodes.Clear();
            GraphState = ENodeStatus.Cancelled;
        }

        public bool IsGraphRunning() {
            return _openNodes.Any();
        }

        private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Node.Progress)) {
                Progress = Nodes.Aggregate(0d, (x, n) => x + n.Progress / Nodes.Count);
            } else if (e.PropertyName == nameof(Node.NodeStatus)) {
                if (Nodes.Any(x => x.NodeStatus == ENodeStatus.Error)) {
                    GraphState = ENodeStatus.Error;
                } else if (Nodes.Any(x => x.NodeStatus == ENodeStatus.Cancelled)) {
                    GraphState = ENodeStatus.Cancelled;
                } else if (Nodes.All(x => x.NodeStatus == ENodeStatus.Completed)) {
                    GraphState = ENodeStatus.Completed;
                    _logger.Information("Graph finished");
                } else {
                    GraphState = ENodeStatus.Running;
                }
            }
        }

        private void OnNodeCompleted(Node node) {
            _openNodes.Remove(node);
            node.NodeCompleted -= OnNodeCompleted;
            node.ResolveParameters -= OnNodeResolveParameters;

            foreach (var result in node.Results) {
                var next = _edgesForward[node.Uid][result.Key];

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

        private void OnNodeResolveParameters(object sender, ResolveParamArgs e) {
            e.Parameters = _media?.Metadata.Parameters;
        }

        public string Name {
            get => _name;
            set => SetField(ref _name, value);
        }

        public string? Base64Image { get; set; }

        [JsonIgnore]
        public string TempPath { get; private set; }

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

            foreach (var node in Nodes.OfType<MediaPartInput>()) {
                node.SetParamStore(Parameters);
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