using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
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
using Newtonsoft.Json;
using Thumbnify.Data;
using Thumbnify.Data.Processing;
using MessageBox = Thumbnify.Dialogs.MessageBox;
using Path = System.IO.Path;

namespace Thumbnify {
    /// <summary>
    /// Interaktionslogik für ProcessingStatus.xaml
    /// </summary>
    public partial class ProcessingStatus : Window {
        public static RoutedUICommand OpenGraph { get; } = new();

        public static readonly DependencyProperty MediaPartsProperty = DependencyProperty.Register(
            nameof(MediaParts), typeof(ObservableCollection<QueueItemStatus>), typeof(ProcessingStatus), new PropertyMetadata(default(ObservableCollection<QueueItemStatus>)));

        public ObservableCollection<QueueItemStatus> MediaParts {
            get { return (ObservableCollection<QueueItemStatus>)GetValue(MediaPartsProperty); }
            set { SetValue(MediaPartsProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress), typeof(double), typeof(ProcessingStatus), new PropertyMetadata(default(double)));

        public double Progress {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public ProcessingStatus() {
            MediaParts = new();

            InitializeComponent();

            CommandBindings.Add(new CommandBinding(OpenGraph, (_, args) => {
                if (args.Parameter is QueueItemStatus item) {
                    var dlg = new GraphViewer();
                    dlg.Owner = this;
                    dlg.Graph = item.Graph;
                    dlg.Show();
                }
            }));
        }

        public void StartProcessing(IEnumerable<MediaPart> parts, bool refetchGraph= true) {
            StartProcessing(parts.Select(x => new QueueItemStatus(x, refetchGraph)));
        }

        public void StartProcessing(IEnumerable<ProcessingGraph> graphs) {
            StartProcessing(graphs.Select(x => new QueueItemStatus(x)));
        }

        public void StartProcessing(IEnumerable<QueueItemStatus> items) {
            MediaParts.Clear();

            foreach (var item in items) {
                var queueItem = item;
                queueItem.Graph.PropertyChanged += (_, args) => {
                    if (args.PropertyName == nameof(ProcessingGraph.Progress)) {
                        Dispatcher.Invoke(() => {
                            Progress = MediaParts.Sum(x => x.Graph.Progress) / MediaParts.Count;
                        });
                    }
                };

                MediaParts.Add(queueItem);
                if (item.MediaPart != null) {
                    queueItem.Graph.RunGraph(item.MediaPart);
                } else {
                    queueItem.Graph.RunGraph();
                }
            }
        }



        private void ProcessingStatus_OnClosed(object? sender, EventArgs e) {
            foreach (var item in MediaParts) {
                if (item.Graph.GraphState == ENodeStatus.Completed) {
                    if (item.MediaPart != null) {
                        item.MediaPart.ProcessingCompleted = true;
                        item.MediaPart.ProcessingCompletedDate = DateTime.Now;
                    }
                }

                try {
                    var tempDir = item.Graph.TempPath;
                    Directory.Delete(tempDir, true);
                } catch (Exception ex) { }

                if (App.Settings.AlwaysSaveLog) {
                    if (!Directory.Exists("logs")) {
                        Directory.CreateDirectory("logs");
                    }

                    var log = new GraphLogBundle(item.Graph);
                    File.WriteAllText(FileTools.SanitizeFilename($"logs\\{DateTime.Now:yyyy-MM-dd} - {item.Name} .json"), JsonConvert.SerializeObject(log));
                }
            }
        }

        private void Close_OnClick(object sender, RoutedEventArgs e) {
            if (MediaParts.Any(x => x.Graph.IsGraphRunning())) {
                if (MessageBox.ShowDialog(this, "cancelGraphs", MessageBoxButton.YesNo) != true) {
                    return;
                }
            }

            foreach (var item in MediaParts) {
                item.Graph.CancelGraph();
            }

            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            if (MediaParts.Any(x => x.Graph.IsGraphRunning())) {
                if (MessageBox.ShowDialog(this, "cancelGraphs", MessageBoxButton.YesNo) != true) {
                    return;
                }
            }

            foreach (var item in MediaParts) {
                item.Graph.CancelGraph();
            }
        }
    }
}