using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xaml.Behaviors.Core;
using Newtonsoft.Json;
using Thumbnify.Controls;
using Thumbnify.Data.Processing;
using Thumbnify.Dialogs;
using Thumbnify.Data;
using Thumbnify.Data.ParamStore;
using Thumbnify.Data.Processing.Audio;
using Thumbnify.Data.Processing.Input;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Postprocessing;
using Connection = Thumbnify.Postprocessing.Connection;
using Connector = Thumbnify.Postprocessing.Connector;
using MessageBox = Thumbnify.Dialogs.MessageBox;
using Node = Thumbnify.Data.Processing.Node;
using PendingConnection = Thumbnify.Postprocessing.PendingConnection;

namespace Thumbnify {
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
            nameof(Graph), typeof(ProcessingGraph), typeof(ProcessingEditor),
            new PropertyMetadata(default(ProcessingGraph)));

        public ProcessingGraph Graph {
            get { return (ProcessingGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        public ICommand AddNode { get; }
        public ICommand CreateParameter { get; }

        public static RoutedUICommand DeleteParameter { get; } = new();

        public static RoutedUICommand MoveParamUp { get; } = new();
        public static RoutedUICommand MoveParamDown { get; } = new();

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

            CommandBindings.Add(new(AudioCompressor.OpenParameters, (_, e) => {
                if (e.Parameter is CompressorParam param) {
                    var dlg = new CompressorParamEditor();
                    dlg.Parameters = param;
                    dlg.Owner = this;
                    dlg.ShowDialog();
                }
            }, (_, e) => { e.CanExecute = e.Parameter is CompressorParam; }));

            CommandBindings.Add(new(MoveParamUp, (_, e) => {
                if (e.Parameter is ParamDefinition def) {
                    var idx = Graph.Parameters.IndexOf(def);

                    if (idx > 0) {
                        Graph.Parameters.Move(idx, idx - 1);
                    }
                }
            }, (_, e) => {
                e.CanExecute = false;
                if (e.Parameter is ParamDefinition def) {
                    e.CanExecute = Graph.Parameters.IndexOf(def) > 0;
                }
            }));

            CommandBindings.Add(new(MoveParamDown, (_, e) => {
                if (e.Parameter is ParamDefinition def) {
                    var idx = Graph.Parameters.IndexOf(def);

                    if (idx < Graph.Parameters.Count - 1) {
                        Graph.Parameters.Move(idx, idx + 1);
                    }
                }
            }, (_, e) => {
                e.CanExecute = false;
                if (e.Parameter is ParamDefinition def) {
                    e.CanExecute = Graph.Parameters.IndexOf(def) < Graph.Parameters.Count - 1;
                }
            }));

            Graph = new();

            InitializeComponent();
        }

        private Dictionary<EditorNode, HashSet<EditorNode>> Tree { get; } = new();

        private void DeleteParameter_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is ParamDefinition def) {
                Graph.Parameters.Remove(def);

                var mediaInput = Graph.Nodes.FirstOrDefault(x => x is MediaPartInput);
                var connections =
                    Graph.ProcessConnects.Where(x => x.Previous == mediaInput.Uid && x.PreviousPort == def.Id).ToList();

                foreach (var connection in connections) {
                    Graph.ProcessConnects.Remove(connection);
                }

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

        private void NewGraph_OnClick(object sender, RoutedEventArgs e) {
            if (Graph.Nodes.Any()) {
                if (MessageBox.ShowDialog(this, "createNew", MessageBoxButton.YesNo) != true) {
                    return;
                }
            }

            Graph = new ProcessingGraph();
        }

        private void FitToScreen_OnClick(object sender, RoutedEventArgs e) {
            Editor.FitAll();
        }

        private void RunGraph_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ProcessingStatus();
            dlg.Owner = this;

            if (Graph.Nodes.Any(x => x is MediaPartInput)) {
                var mediaPart = new MediaPart();
                var metadata = new PartMetadata();
                metadata.Parameters = new();
                foreach (var param in Graph.Parameters) {
                    metadata.Parameters.Add(param.Clone());
                }

                var metaDlg = new GraphParameters();
                metaDlg.Owner = this;
                metaDlg.PartMetadata = metadata;

                if (metaDlg.ShowDialog() == true) {
                    if (File.Exists(metaDlg.MediaFile)) {
                        mediaPart.Duration = -1;
                        mediaPart.Start = 0;
                        mediaPart.End = -1;
                        mediaPart.Parent = new MediaSource {
                            FileName = metaDlg.MediaFile
                        };
                        mediaPart.Metadata = metadata;

                        dlg.StartProcessing([mediaPart], false);
                    }
                } else {
                    return;
                }
            } else {
                dlg.StartProcessing(new[] { Graph });
            }

            dlg.ShowDialog();
        }
    }
}