﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using tebisCloud.Data;

namespace tebisCloud {
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

        public ProcessingStatus() {
            MediaParts = new();

            InitializeComponent();

            CommandBindings.Add(new CommandBinding(OpenGraph, (_, args) => {
                if (args.Parameter is QueueItemStatus item) {
                    var dlg = new GraphViewer();
                    dlg.Owner = this;
                    dlg.Graph = item.MediaPart.Metadata.ProcessingGraph;
                    dlg.Show();
                }
            }));
        }

        public void StartProcessing(IEnumerable<MediaPart> parts) {
            MediaParts.Clear();

            foreach (var part in parts) {
                MediaParts.Add(new QueueItemStatus(part));
                part.Metadata.ProcessingGraph.RunGraph(part);
            }
        }
    }
}