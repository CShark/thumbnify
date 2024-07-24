﻿using JsonKnownTypes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Serilog;
using Thumbnify.Postprocessing;
using Point = System.Windows.Point;

namespace Thumbnify.Data.Processing {
    public enum ENodeStatus {
        Pending,
        Running,
        Completed,
        Cancelled,
        Error
    }

    [JsonConverter(typeof(JsonKnownTypesConverter<Node>))]
    public abstract class Node : INotifyPropertyChanged {
        private Point _nodeLocation;
        private double _progress;
        private ENodeStatus _nodeStatus = ENodeStatus.Pending;
        private bool _isExpanded = true;
        private ILogger _logger;

        private Dictionary<string, Parameter> _parameters = new();
        private Dictionary<string, Result> _results = new();

        public event Action<Node> NodeCompleted;

        public event Action PortsChanged;

        public string Uid { get; set; }

        [JsonIgnore]
        public IReadOnlyDictionary<string, Parameter> Parameters => _parameters;

        [JsonIgnore]
        public IReadOnlyDictionary<string, Result> Results => _results;

        [JsonIgnore]
        protected abstract ENodeType NodeType { get; }

        [JsonIgnore]
        protected abstract string NodeId { get; }

        [JsonIgnore]
        public ENodeStatus NodeStatus {
            get => _nodeStatus;
            private set => SetField(ref _nodeStatus, value);
        }

        [JsonIgnore]
        protected ILogger Logger { get; private set; }

        public bool IsExpanded {
            set => SetField(ref _isExpanded, value);
            get => _isExpanded;
        }

        [JsonIgnore]
        public string Messages { get; private set; } = "";

        [JsonIgnore]
        public CancellationToken CancelToken { get; set; }

        [JsonIgnore]
        public double Progress {
            get => _progress;
            private set => SetField(ref _progress, value);
        }

        public Point NodeLocation {
            get => _nodeLocation;
            set => SetField(ref _nodeLocation, value);
        }

        protected Node() {
            Uid = Guid.NewGuid().ToString();
        }

        protected void RegisterParameter(Parameter param) {
            _parameters.Add(param.Id, param);
            param.ValueChanged += ParamOnValueChanged;
        }

        protected void RegisterResult(Result result) {
            _results.Add(result.Id, result);
        }

        protected void ClearResults() {
            _results.Clear();
        }

        protected void ClearParameters() {
            _parameters.Clear();
        }

        private void ParamOnValueChanged() {
            lock (this) {
                if (CancelToken.IsCancellationRequested) return;

                if (NodeStatus == ENodeStatus.Pending) {
                    if (Parameters.Values.All(x => x.HasValue)) {
                        Task.Run(() => {
                            NodeStatus = ENodeStatus.Running;
                            try {
                                NodeStatus = Execute(CancelToken) ? ENodeStatus.Completed : ENodeStatus.Error;
                                Logger.Information("Node completed");

                                foreach (var param in Parameters.Values) {
                                    param.Clear();
                                }
                            } catch (Exception ex) {
                                Logger.Error(ex, "Node execution failed");
                                NodeStatus = ENodeStatus.Error;
                            }

                            Progress = 1;
                            OnNodeCompleted();
                        }, CancelToken);
                    }
                }
            }
        }

        public void ClearNode() {
            foreach (var param in Parameters.Values) {
                param.Clear();
            }

            foreach (var result in Results.Values) {
                result.Clear();
            }

            NodeStatus = ENodeStatus.Pending;
            Messages = "";
            Progress = 0;
        }

        public void TriggerNode() {
            ParamOnValueChanged();
        }

        public void SetLogger(ILogger logger) {
            Logger = logger;
        }

        protected abstract bool Execute(CancellationToken cancelToken);
        
        protected void ReportProgress(double progress) {
            Progress = progress;
        }

        protected void ReportProgress(long step, long max) {
            Progress = (double)step / max;
        }

        public EditorNode GenerateNode() {
            return new EditorNode(NodeId, NodeType, this);
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

        protected virtual void OnNodeCompleted() {
            NodeCompleted?.Invoke(this);
        }

        protected virtual void OnPortsChanged() {
            PortsChanged?.Invoke();
        }
    }
}