using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using tebisCloud.Data.ParamStore;

namespace tebisCloud.Data {
    public class PartMetadata :INotifyPropertyChanged{
        private ProcessingGraph? _processingGraph;
        private ObservableCollection<ParamDefinition> _parameters = new();
        private string _name;

        public string Name {
            get => _name;
            set => SetField(ref _name, value);
        }

        public ProcessingGraph? ProcessingGraph {
            get => _processingGraph;
            set => SetField(ref _processingGraph, value);
        }


        public ObservableCollection<ParamDefinition> Parameters {
            get => _parameters;
            set => SetField(ref _parameters, value);
        }

        public void UpdateParameters() {
            var paramList = Parameters.ToList();

            Parameters.Clear();
            foreach (var param in _processingGraph.Parameters) {
                var clone = param.Clone();

                var orig = paramList.FirstOrDefault(x =>
                    (x.Id == clone.Id || x.Name.ToLower() == clone.Name.ToLower()) && x.Type == clone.Type);

                if (orig != null) {
                    clone.Value = orig.Value;
                }

                Parameters.Add(clone);
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