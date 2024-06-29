using JsonKnownTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using tebisCloud.Data.Thumbnail;
using tebisCloud.Postprocessing;
using Vortice.XAPO;
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

        public event Action<Node> NodeCompleted;

        public string Id { get; }

        public abstract IReadOnlyDictionary<string, Parameter> Parameters { get; }
        public abstract IReadOnlyDictionary<string, Result> Results { get; }

        [JsonIgnore]
        public ENodeStatus NodeStatus { get; private set; } = ENodeStatus.Pending;

        [JsonIgnore]
        public string Messages { get; private set; } = "";

        [JsonIgnore]
        public CancellationToken CancelToken { get; set; }

        public Point NodeLocation {
            get => _nodeLocation;
            set => SetField(ref _nodeLocation, value);
        }

        protected Node() {
            Id = Guid.NewGuid().ToString();
        }

        protected void InitializeParameters() {
            foreach (var param in Parameters.Values) {
                param.ValueChanged += ParamOnValueChanged;
            }
        }

        private void ParamOnValueChanged() {
            lock (this) {
                if (CancelToken.IsCancellationRequested) return;

                if (NodeStatus == ENodeStatus.Pending) {
                    if (Parameters.Values.All(x => x.HasValue)) {
                        NodeStatus = ENodeStatus.Running;
                        NodeStatus = Execute(CancelToken) ? ENodeStatus.Completed : ENodeStatus.Error;

                        foreach (var param in Parameters.Values) {
                            param.Clear();
                        }

                        OnNodeCompleted();
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
        }

        public void TriggerNode() {
            ParamOnValueChanged();
        }

        protected abstract bool Execute(CancellationToken cancelToken);

        protected void LogMessage(string message) {
            Messages += message + "\n\n";
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