using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using Serilog.Events;
using Thumbnify.Data;
using Thumbnify.Postprocessing;
using Thumbnify.Tools;
using Vortice.XAPO;
using Path = System.IO.Path;

namespace Thumbnify {
    /// <summary>
    /// Interaktionslogik für GraphViewer.xaml
    /// </summary>
    public partial class GraphViewer : Window {
        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register(
            nameof(Graph), typeof(ProcessingGraph), typeof(GraphViewer),
            new PropertyMetadata(default(ProcessingGraph), (o, args) => {
                var obj = ((GraphViewer)o);

                if (args.NewValue != null) {
                    var view = new ListCollectionView(((ProcessingGraph)args.NewValue).LogMessages.MessageList);

                    obj.LogMessages = view;
                    view.Filter = x => {
                        var shown = obj._logFilter.Contains(((LogMessage)x).NodeUid);

                        if (shown) {
                            shown = ((LogMessage)x).Level switch {
                                LogEventLevel.Debug => obj.ShowDebug,
                                LogEventLevel.Information => obj.ShowInfo,
                                LogEventLevel.Warning => obj.ShowWarn,
                                LogEventLevel.Error => obj.ShowError,
                                _ => true
                            };
                        }

                        return shown;
                    };

                    obj.Selection_OnCollectionChanged(null, null);
                } else {
                    obj.LogMessages = null;
                }

                obj.SelectedNodes.Clear();
            }));

        public ProcessingGraph Graph {
            get { return (ProcessingGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        public static readonly DependencyProperty SelectedNodesProperty = DependencyProperty.Register(
            nameof(SelectedNodes), typeof(ObservableCollection<EditorNode>), typeof(GraphViewer), new PropertyMetadata(
                default(ObservableCollection<EditorNode>),
                (o, args) => {
                    if (args.OldValue is ObservableCollection<EditorNode> oldValue) {
                        oldValue.CollectionChanged -= ((GraphViewer)o).Selection_OnCollectionChanged;
                    }

                    if (args.NewValue is ObservableCollection<EditorNode> newValue) {
                        newValue.CollectionChanged += ((GraphViewer)o).Selection_OnCollectionChanged;
                    }
                }));

        public ObservableCollection<EditorNode> SelectedNodes {
            get { return (ObservableCollection<EditorNode>)GetValue(SelectedNodesProperty); }
            set { SetValue(SelectedNodesProperty, value); }
        }

        public static readonly DependencyProperty SelectedLogMessageProperty = DependencyProperty.Register(
            nameof(SelectedLogMessage), typeof(LogMessage), typeof(GraphViewer),
            new PropertyMetadata(default(LogMessage?)));

        public LogMessage? SelectedLogMessage {
            get { return (LogMessage?)GetValue(SelectedLogMessageProperty); }
            set { SetValue(SelectedLogMessageProperty, value); }
        }

        public static readonly DependencyProperty LogMessagesProperty = DependencyProperty.Register(
            nameof(LogMessages), typeof(ICollectionView), typeof(GraphViewer),
            new PropertyMetadata(default(ICollectionView?)));

        public ICollectionView? LogMessages {
            get { return (ICollectionView?)GetValue(LogMessagesProperty); }
            set { SetValue(LogMessagesProperty, value); }
        }

        public static readonly DependencyProperty ShowDebugProperty = DependencyProperty.Register(
            nameof(ShowDebug), typeof(bool), typeof(GraphViewer), new PropertyMetadata(false,
                (o, _) => ((GraphViewer)o).LogMessages?.Refresh()));

        public bool ShowDebug {
            get { return (bool)GetValue(ShowDebugProperty); }
            set { SetValue(ShowDebugProperty, value); }
        }

        public static readonly DependencyProperty ShowInfoProperty = DependencyProperty.Register(
            nameof(ShowInfo), typeof(bool), typeof(GraphViewer), new PropertyMetadata(true,
                (o, _) => ((GraphViewer)o).LogMessages?.Refresh()));

        public bool ShowInfo {
            get { return (bool)GetValue(ShowInfoProperty); }
            set { SetValue(ShowInfoProperty, value); }
        }

        public static readonly DependencyProperty ShowWarnProperty = DependencyProperty.Register(
            nameof(ShowWarn), typeof(bool), typeof(GraphViewer), new PropertyMetadata(true,
                (o, _) => ((GraphViewer)o).LogMessages?.Refresh()));

        public bool ShowWarn {
            get { return (bool)GetValue(ShowWarnProperty); }
            set { SetValue(ShowWarnProperty, value); }
        }

        public static readonly DependencyProperty ShowErrorProperty = DependencyProperty.Register(
            nameof(ShowError), typeof(bool), typeof(GraphViewer), new PropertyMetadata(true,
                (o, _) => ((GraphViewer)o).LogMessages?.Refresh()));

        public bool ShowError {
            get { return (bool)GetValue(ShowErrorProperty); }
            set { SetValue(ShowErrorProperty, value); }
        }

        public GraphViewer() {
            SelectedNodes = new();

            InitializeComponent();
        }

        private bool _updatingSelection = false;
        private List<string> _logFilter = new();

        private void Selection_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            if (_updatingSelection) return;
            _updatingSelection = true;

            if (Mouse.LeftButton == MouseButtonState.Released || SelectedNodes.Count != 0) {
                _logFilter.Clear();

                if (SelectedNodes.Count == 0) {
                    _logFilter.AddRange(Graph.Nodes.Select(x => x.Uid));
                    _logFilter.Add(null);
                } else {
                    _logFilter.AddRange(SelectedNodes.Select(x => x.SourceNode.Uid));
                }

                LogMessages.Refresh();
            }

            _updatingSelection = false;
        }

        private void LogMessages_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (_updatingSelection) return;
            _updatingSelection = true;

            SelectedNodes.Clear();

            if (SelectedLogMessage?.NodeUid != null) {
                var node = GraphControl.Nodes.FirstOrDefault(x => x.SourceNode.Uid == SelectedLogMessage.NodeUid);

                if (node != null) {
                    SelectedNodes.Add(node);
                    GraphControl.BringIntoView(node);
                }
            }

            _updatingSelection = false;
        }

        private void ExportLog_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new VistaSaveFileDialog {
                Title = Translate.TranslateControl("LogExport"),
                Filter = "JSON-Files|*.json",
                AddExtension = true
            };

            if (dlg.ShowDialog(this) == true) {
                if (!dlg.FileName.ToLower().EndsWith(".json")) {
                    dlg.FileName += ".json";
                }

                var logBundle = new GraphLogBundle(Graph);
                var json = JsonConvert.SerializeObject(logBundle);

                File.WriteAllText(dlg.FileName, json);
            }
        }

        private void ImportLog_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new VistaOpenFileDialog {
                Title=Translate.TranslateControl("LogImport"),
                Filter="JSON-Files|*.json",
                AddExtension = true
            };

            if (dlg.ShowDialog(this) == true) {
                var json = File.ReadAllText(dlg.FileName);
                var bundle = JsonConvert.DeserializeObject<GraphLogBundle>(json);

                if (bundle != null) {
                    Graph = bundle.Graph;
                }
            }
        }
    }
}