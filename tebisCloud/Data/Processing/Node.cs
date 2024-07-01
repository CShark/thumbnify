using JsonKnownTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Newtonsoft.Json;
using tebisCloud.Data.Thumbnail;
using tebisCloud.Postprocessing;
using Application = System.Windows.Application;
using Point = System.Windows.Point;

namespace tebisCloud.Data.Processing {
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

        public event Action<Node> NodeCompleted;

        public string Id { get; set; }

        [JsonIgnore]
        public abstract IReadOnlyDictionary<string, Parameter> Parameters { get; protected set; }

        [JsonIgnore]
        public abstract IReadOnlyDictionary<string, Result> Results { get; protected set; }

        [JsonIgnore]
        public ENodeStatus NodeStatus {
            get => _nodeStatus;
            private set => SetField(ref _nodeStatus, value);
        }

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
            Parameters = new Dictionary<string, Parameter>();
            Results = new Dictionary<string, Result>();
            Id = Guid.NewGuid().ToString();
        }

        protected void Initialize() {
            OnDeserialized(new StreamingContext());
        }

        protected abstract void InitializeParamsResults();

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) {
            foreach (var param in Parameters.Values) {
                param.ValueChanged -= ParamOnValueChanged;
            }
            InitializeParamsResults();
            foreach (var param in Parameters.Values) {
                param.ValueChanged += ParamOnValueChanged;
            }
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

                                foreach (var param in Parameters.Values) {
                                    param.Clear();
                                }
                            } catch (Exception ex) {
                                LogMessage("Node execution failed: " + ex);
                                NodeStatus = ENodeStatus.Error;
                            }
                            Progress = 1000;
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

        protected abstract bool Execute(CancellationToken cancelToken);

        protected void LogMessage(string message) {
            Messages += message + "\n\n";
        }

        protected void ReportProgress(double progress) {
            Progress = progress;
        }

        protected void ReportProgress(long step, long max) {
            Progress = (double)step  / max;
        }

        public abstract EditorNode GenerateNode();


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
    }
}