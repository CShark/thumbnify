using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using Thumbnify.Data;
using Thumbnify.Postprocessing;

namespace Thumbnify {
    /// <summary>
    /// Interaktionslogik für GraphViewer.xaml
    /// </summary>
    public partial class GraphViewer : Window {
        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register(
            nameof(Graph), typeof(ProcessingGraph), typeof(GraphViewer),
            new PropertyMetadata(default(ProcessingGraph)));

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

        private void Selection_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            
        }

        public ObservableCollection<EditorNode> SelectedNodes {
            get { return (ObservableCollection<EditorNode>)GetValue(SelectedNodesProperty); }
            set { SetValue(SelectedNodesProperty, value); }
        }


        public GraphViewer() {
            SelectedNodes = new();

            InitializeComponent();
        }
    }
}