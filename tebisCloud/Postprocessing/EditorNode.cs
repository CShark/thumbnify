using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using tebisCloud.Data.Processing;
using Vortice.XAudio2;

namespace tebisCloud.Postprocessing {
    [Flags]
    public enum ENodeType {
        None = 0,
        Audio = 1,
        Video = 2,
        Youtube = 4
    }

    public class EditorNode : INotifyPropertyChanged {
        private List<Connector> _inputs = new();
        private List<Connector> _outputs = new();

        public static Dictionary<ENodeType, Color> TypeColorMap { get; } = new() {
            { ENodeType.None, Colors.DimGray },
            { ENodeType.Audio, Colors.SteelBlue },
            { ENodeType.Video, Colors.MediumOrchid },
            { ENodeType.Youtube, Colors.Red }
        };

        public EditorNode(string title, ENodeType nodeType, Node step) {
            Title = title;
            Step = step;
            NodeType = nodeType;

            _inputs = step.Parameters.Values.Select(x => new Connector(this, x)).ToList();
            _outputs = step.Results.Values.Select(x => new Connector(this, x)).ToList();

            NodeColor = TypeColorMap[NodeType];
        }
        
        public string Title { get; }

        public ENodeType NodeType { get; }

        public Node Step { get; }

        public Color NodeColor { get; }

        public IReadOnlyList<Connector> Inputs => _inputs;
        public IReadOnlyList<Connector> Outputs => _outputs;

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