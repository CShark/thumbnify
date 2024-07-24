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
    }

    public class EditorNode : INotifyPropertyChanged {
        private List<Connector> _inputs = new();
        private List<Connector> _outputs = new();
        private List<Parameter> _staticParameters = new();
        private double _progress;

        public static Dictionary<ENodeType, Color> TypeColorMap { get; } = new() {
            { ENodeType.None, Colors.DimGray },
            { ENodeType.Audio, Colors.DarkOrange },
            { ENodeType.Video, Colors.MediumOrchid },
            { ENodeType.Parameter, Colors.CornflowerBlue }
        };

        public EditorNode(string titleId, ENodeType nodeType, Node sourceNode) {
            TitleId = titleId;
            SourceNode = sourceNode;
            NodeType = nodeType;

            _inputs = sourceNode.Parameters.Values.Where(x => x.Bindable).Select(x => new Connector(this, x)).ToList();
            _outputs = sourceNode.Results.Values.Select(x => new Connector(this, x)).ToList();
            _staticParameters = sourceNode.Parameters.Values.Where(x => !x.Bindable).ToList();

            NodeColor = TypeColorMap[NodeType];
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
    }
}