using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Thumbnify.Data.Processing;
using Vortice.XAudio2;

namespace Thumbnify.Postprocessing {
    public enum ENodeType {
        None = 0,
        Audio = 1,
        Video = 2,
        Parameter = 3,
        Youtube = 4,
    }

    public class EditorNode : IDisposable, INotifyPropertyChanged {
        private List<Connector> _inputs = new();
        private List<Connector> _outputs = new();
        private List<Parameter> _staticParameters = new();
        private double _progress;

        public static Dictionary<ENodeType, Color> TypeColorMap { get; } = new() {
            { ENodeType.None, Colors.DimGray },
            { ENodeType.Audio, Colors.DarkOrange },
            { ENodeType.Video, Colors.MediumOrchid },
            { ENodeType.Parameter, Colors.CornflowerBlue },
            { ENodeType.Youtube, Colors.Red }
        };

        public EditorNode(string titleId, ENodeType nodeType, Node sourceNode) {
            TitleId = titleId;
            SourceNode = sourceNode;
            NodeType = nodeType;
            NodeColor = TypeColorMap[NodeType];

            SourceNode.PortsChanged += SourceNodeOnPortsChanged;
            SourceNodeOnPortsChanged();
        }

        private void SourceNodeOnPortsChanged() {
            var inConnectors = _inputs.ToList();
            var outConnectors = _outputs.ToList();

            var inputs = SourceNode.Parameters.Values.Where(x => x.Bindable);
            var outputs = SourceNode.Results.Values;

            foreach (var input in inputs) {
                var orig = inConnectors.FirstOrDefault(x => x.Parameter.Id == input.Id);

                if (orig != null) {
                    orig.Parameter = input;
                }
            }

            foreach (var output in outputs) {
                var orig = outConnectors.FirstOrDefault(x => x.Result.Id == output.Id);

                if (orig != null) {
                    orig.Result = output;
                }
            }


            _inputs = SourceNode.Parameters.Values.Where(x => x.Bindable).Select(x =>
                inConnectors.FirstOrDefault(y => y.Parameter == x) ?? new Connector(this, x)).ToList();
            _outputs = SourceNode.Results.Values
                .Select(x => outConnectors.FirstOrDefault(y => y.Result == x) ?? new Connector(this, x)).ToList();


            _staticParameters = SourceNode.Parameters.Values.Where(x => !x.Bindable).ToList();

            OnPropertyChanged(nameof(Inputs));
            OnPropertyChanged(nameof(Outputs));
            OnPropertyChanged(nameof(StaticParameters));
        }

        public string TitleId { get; }

        public ENodeType NodeType { get; }

        public Node SourceNode { get; }

        public Color NodeColor { get; }

        public IReadOnlyList<Connector> Inputs => _inputs;
        public IReadOnlyList<Connector> Outputs => _outputs;

        public IReadOnlyList<Parameter> StaticParameters => _staticParameters;

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

        public void Dispose() {
            SourceNode.PortsChanged -= SourceNodeOnPortsChanged;
        }
    }
}