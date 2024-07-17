using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using Newtonsoft.Json;
using tebisCloud.Data;
using tebisCloud.Data.ParamStore;
using tebisCloud.Data.Processing;
using tebisCloud.Dialogs;
using tebisCloud.Postprocessing;
using Connection = tebisCloud.Postprocessing.Connection;
using Connector = tebisCloud.Postprocessing.Connector;
using MessageBox = tebisCloud.Dialogs.MessageBox;
using Node = tebisCloud.Data.Processing.Node;
using PendingConnection = tebisCloud.Postprocessing.PendingConnection;

namespace tebisCloud {
    /// <summary>
    /// Interaktionslogik für ProcessingEditor.xaml
    /// </summary>
    public partial class ProcessingEditor : Window {
        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(
            nameof(Parameters), typeof(ObservableCollection<ParamDefinition>), typeof(ProcessingEditor),
            new PropertyMetadata(default(ObservableCollection<ParamDefinition>)));

        public ObservableCollection<ParamDefinition> Parameters {
            get { return (ObservableCollection<ParamDefinition>)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        public static readonly DependencyProperty SelectedParamProperty = DependencyProperty.Register(
            nameof(SelectedParam), typeof(ParamDefinition), typeof(ProcessingEditor),
            new PropertyMetadata(default(ParamDefinition)));

        public ParamDefinition SelectedParam {
            get { return (ParamDefinition)GetValue(SelectedParamProperty); }
            set { SetValue(SelectedParamProperty, value); }
        }

        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register(
            nameof(Graph), typeof(ProcessingGraph), typeof(ProcessingEditor), new PropertyMetadata(default(ProcessingGraph)));

        public ProcessingGraph Graph {
            get { return (ProcessingGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        public ICommand AddNode { get; }
        public ICommand CreateParameter { get; }

        public static RoutedUICommand DeleteParameter { get; } = new();

        public ProcessingEditor() {
            Parameters = new();

            CreateParameter = new ActionCommand(() => {
                var def = ParamDefCreate.ShowDialog(this);

                if (def != null) {
                    Graph.Parameters.Add(def);
                    Parameters.Add(def);
                    Editor.RebuildGraph();
                }
            });

            AddNode = new ActionCommand(() => {
                ContextMenu.PlacementTarget = this;
                ContextMenu.IsOpen = true;
            });

            Graph = new();

            InitializeComponent();
        }

        private Dictionary<EditorNode, HashSet<EditorNode>> Tree { get; } = new();

        private void DeleteParameter_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is ParamDefinition def) {
                Parameters.Remove(def);
                Graph.Parameters.Remove(def);
                Editor.RebuildGraph();
            }
        }

        private void DeleteParameter_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = e.Parameter is ParamDefinition;
        }
    }
}