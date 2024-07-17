using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using tebisCloud.Data.Processing;

namespace tebisCloud.Postprocessing {
    public class Connector : INotifyPropertyChanged {
        private Point _anchor;
        private bool _isConnected;

        public Type Type { get; }

        public string Id { get; }

        public Parameter? Parameter { get; }
        public Result? Result { get; }

        public Point Anchor {
            get => _anchor;
            set => SetField(ref _anchor, value);
        }

        public bool IsConnected {
            get => _isConnected;
            set => SetField(ref _isConnected, value);
        }

        public EditorNode Parent { get; }

        public List<Connection> Connections { get; } = new();
        
        public Connector(EditorNode parent, Parameter param) {
            Type = param.Type;
            Parent = parent;
            Id = param.Id;
            Parameter = param;
        }

        public Connector(EditorNode parent, Result result) {
            Type = result.Type;
            Parent = parent;
            Id = result.Id;
            Result = result;
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