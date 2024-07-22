﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xaml.Behaviors.Core;
using Newtonsoft.Json;
using tebisCloud.Controls;
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
            CreateParameter = new ActionCommand(() => {
                var def = ParamDefCreate.ShowDialog(this);

                if (def != null) {
                    Graph.Parameters.Add(def);
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
                Graph.Parameters.Remove(def);
                Editor.RebuildGraph();
            }
        }

        private void DeleteParameter_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = e.Parameter is ParamDefinition;
        }

        private void GraphSave_OnClick(object sender, RoutedEventArgs e) {
            var result = LoadSaveDialog.ShowSaveDialog(this, App.Settings.Processing, x => {
                App.Settings.Processing.Remove(x);
                App.SaveSettings();
            });

            if (result != null) {
                var json = JsonConvert.SerializeObject(Graph);
                var copy = JsonConvert.DeserializeObject<ProcessingGraph>(json);

                var preview = new GraphEditor();
                preview.Graph = copy;
                preview.Measure(new Size(1920, 1080));
                preview.Arrange(new Rect(0, 0, 1920, 1080));
                preview.UpdateLayout();
                preview.FitAll();

                var render = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
                render.Render(preview);
                copy.Preview = render;
                copy.Name = result;

                var orig = App.Settings.Processing.FirstOrDefault(x => x.Name.ToLower() == result.ToLower());

                if (orig != null) {
                    App.Settings.Processing.Remove(orig);
                }

                App.Settings.Processing.Add(copy);
                App.SaveSettings();
            }
        }

        private void GraphLoad_OnClick(object sender, RoutedEventArgs e) {
            var result = LoadSaveDialog.ShowOpenDialog(this, App.Settings.Processing, x => {
                App.Settings.Processing.Remove(x);
                App.SaveSettings();
            });

            if (result != null) {
                var json = JsonConvert.SerializeObject(result);
                var copy = JsonConvert.DeserializeObject<ProcessingGraph>(json);

                Graph = copy;
                Editor.FitAll();
            }
        }
    }
}