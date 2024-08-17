using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nodify;
using Thumbnify.Data;
using Thumbnify.Data.Processing;
using Thumbnify.Data.Processing.Input;
using Thumbnify.Postprocessing;
using Thumbnify.Tools;
using Connection = Thumbnify.Postprocessing.Connection;
using Connector = Thumbnify.Postprocessing.Connector;
using MessageBox = Thumbnify.Dialogs.MessageBox;
using Node = Thumbnify.Data.Processing.Node;
using PendingConnection = Thumbnify.Postprocessing.PendingConnection;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für GraphEditor.xaml
    /// </summary>
    public partial class GraphEditor : UserControl {
        public static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
            nameof(Nodes), typeof(ObservableCollection<EditorNode>), typeof(GraphEditor),
            new PropertyMetadata(default(ObservableCollection<EditorNode>)));

        public ObservableCollection<EditorNode> Nodes {
            get { return (ObservableCollection<EditorNode>)GetValue(NodesProperty); }
            set { SetValue(NodesProperty, value); }
        }

        public static readonly DependencyProperty ConnectionsProperty = DependencyProperty.Register(
            nameof(Connections), typeof(ObservableCollection<Connection>), typeof(GraphEditor),
            new PropertyMetadata(default(ObservableCollection<Connection>)));

        public ObservableCollection<Connection> Connections {
            get { return (ObservableCollection<Connection>)GetValue(ConnectionsProperty); }
            set { SetValue(ConnectionsProperty, value); }
        }

        public static readonly DependencyProperty PendingConnectionProperty = DependencyProperty.Register(
            nameof(PendingConnection), typeof(PendingConnection), typeof(GraphEditor),
            new PropertyMetadata(default(PendingConnection)));

        public PendingConnection PendingConnection {
            get { return (PendingConnection)GetValue(PendingConnectionProperty); }
            set { SetValue(PendingConnectionProperty, value); }
        }

        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register(
            nameof(Graph), typeof(ProcessingGraph), typeof(GraphEditor),
            new PropertyMetadata(default(ProcessingGraph), (o, args) => {
                if (args.OldValue is ProcessingGraph gOld) {
                    gOld.Nodes.CollectionChanged -= ((GraphEditor)o).Nodes_CollectionChanged;
                    gOld.ProcessConnects.CollectionChanged -= ((GraphEditor)o).Connections_CollectionChanged;
                }

                if (args.NewValue is ProcessingGraph gNew) {
                    gNew.Nodes.CollectionChanged += ((GraphEditor)o).Nodes_CollectionChanged;
                    gNew.ProcessConnects.CollectionChanged += ((GraphEditor)o).Connections_CollectionChanged;
                }

                ((GraphEditor)o).RebuildGraph();
                ((GraphEditor)o).FitAll();
            }));

        public ProcessingGraph Graph {
            get { return (ProcessingGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        public static readonly DependencyProperty SelectedNodesProperty = DependencyProperty.Register(
            nameof(SelectedNodes), typeof(ObservableCollection<EditorNode>), typeof(GraphEditor),
            new PropertyMetadata(default(ObservableCollection<EditorNode>)));

        public ObservableCollection<EditorNode> SelectedNodes {
            get { return (ObservableCollection<EditorNode>)GetValue(SelectedNodesProperty); }
            set { SetValue(SelectedNodesProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly), typeof(bool), typeof(GraphEditor), new PropertyMetadata(default(bool)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static RoutedUICommand DisconnectNodes { get; } = new();
        public static RoutedUICommand CreateNode { get; } = new();
        public static RoutedUICommand DeleteNode { get; } = new();

        private Dictionary<EditorNode, HashSet<EditorNode>> Tree { get; } = new();

        public GraphEditor() {
            Nodes = new();
            Connections = new();
            SelectedNodes = new();

            NodifyEditor.EnableCuttingLinePreview = true;
            EditorGestures.Mappings.Editor.Cutting.Value = MultiGesture.None;

            PendingConnection = new PendingConnection((a, b) => {
                if (a.Parameter != null) {
                    (a, b) = (b, a);
                }

                if (a.Type == b.Type) {
                    if (a.Parameter == null && b.Result == null &&
                        !Connections.Any(x => (x.Target == b && x.Source == a) || x.Target == b)) {
                        var con = new Connection(a, b, a.Type);
                        if (!IsCyclic(con)) {
                            Graph.ProcessConnects.Add(new() {
                                Previous = a.Parent.SourceNode.Uid,
                                PreviousPort = a.Id,
                                Next = b.Parent.SourceNode.Uid,
                                NextPort = b.Id,
                            });
                        }
                    }
                }
            });

            CommandBindings.Add(new CommandBinding(DisconnectNodes, (_, args) => {
                if (args.Parameter is Connector point) {
                    var conn = Connections.FirstOrDefault(x => x.Target == point || x.Source == point);

                    if (conn != null) {
                        var dataConn = Graph.ProcessConnects.FirstOrDefault(x =>
                            x.Previous == conn.Source.Parent.SourceNode.Uid &&
                            x.PreviousPort == conn.Source.Id &&
                            x.Next == conn.Target.Parent.SourceNode.Uid &&
                            x.NextPort == conn.Target.Id
                        );

                        Graph.ProcessConnects.Remove(dataConn);
                    }
                }
            }));

            CommandBindings.Add(new CommandBinding(DeleteNode, (_, _) => {
                if (((MultiSelector)Editor).SelectedItems.Count > 0) {
                    if (Dialogs.MessageBox.ShowDialog(Window.GetWindow(this), "deleteActions",
                            MessageBoxButton.YesNo) == true) {
                        var selection = new List<EditorNode>();

                        foreach (EditorNode node in ((MultiSelector)Editor).SelectedItems) {
                            selection.Add(node);
                        }

                        foreach (var node in selection) {
                            Graph.RemoveNode(node.SourceNode);
                        }

                        Editor.SelectedItem = null;
                    }
                }
            }, (_, args) => { args.CanExecute = ((MultiSelector)Editor).SelectedItems.Count > 0; }));

            CommandBindings.Add(new CommandBinding(CreateNode, (_, args) => {
                if (args.Parameter is Type t) {
                    if (t.IsAssignableTo(typeof(Node))) {
                        if (t.IsAssignableFrom(typeof(MediaPartInput))) {
                            if (Graph.Nodes.Count(x => x is MediaPartInput) > 0) {
                                return;
                            }
                        }

                        if (Activator.CreateInstance(t) is Node step) {
                            Editor.ViewportTransform.Inverse.TryTransform(Mouse.GetPosition(Editor), out var point);
                            step.NodeLocation = point;
                            Graph.AddNode(step);
                        }
                    }
                }
            }));

            Nodes.CollectionChanged += (_, args) => {
                if (args.OldItems != null) {
                    foreach (EditorNode node in args.OldItems) {
                        Tree.Remove(node);

                        foreach (var item in Tree.Values) {
                            item.Remove(node);
                        }

                        var conns = Connections.Where(x => x.Source.Parent == node || x.Target.Parent == node).ToList();

                        foreach (var conn in conns) {
                            Connections.Remove(conn);
                        }
                    }
                }

                if (args.NewItems != null) {
                    foreach (EditorNode node in args.NewItems) {
                        Tree[node] = new();
                    }
                }
            };

            Connections.CollectionChanged += (_, args) => {
                if (args.OldItems != null) {
                    foreach (Connection conn in args.OldItems) {
                        if (Tree.ContainsKey(conn.Source.Parent)) {
                            Tree[conn.Source.Parent].Remove(conn.Target.Parent);
                        }

                        conn.Source.IsConnected = Connections.Any(x => x.Source == conn.Source);
                        conn.Target.IsConnected = Connections.Any(x => x.Target == conn.Target);

                        conn.Source.Connections.Remove(conn);
                        conn.Target.Connections.Remove(conn);
                    }
                }

                if (args.NewItems != null) {
                    foreach (Connection conn in args.NewItems) {
                        conn.Source.IsConnected = true;
                        conn.Target.IsConnected = true;

                        if (Tree.ContainsKey(conn.Source.Parent)) {
                            Tree[conn.Source.Parent].Add(conn.Target.Parent);
                        }

                        conn.Source.Connections.Add(conn);
                        conn.Target.Connections.Add(conn);
                    }
                }
            };

            InitializeComponent();

            Unloaded += (_, _) => {
                if (Graph != null) {
                    Graph.Nodes.CollectionChanged -= Nodes_CollectionChanged;
                    Graph.ProcessConnects.CollectionChanged -= Connections_CollectionChanged;
                }
            };

            Loaded += (sender, args) => { FitAll(); };
            Unloaded += (sender, args) => {
                foreach (var node in Nodes) {
                    node.Dispose();
                }
            };
        }

        private bool IsCyclic(Connection? pending = null) {
            // Kahns' Algorithm
            var connections = Connections.ToList();
            if (pending != null) {
                connections = connections.Concat([pending]).ToList();
            }

            Dictionary<EditorNode, int> nodeDegree =
                Nodes.ToDictionary(x => x, x => connections.Count(y => y.Target.Parent == x));

            var queue = new Queue<EditorNode>(nodeDegree.Where(x => x.Value == 0).Select(x => x.Key).ToList());


            while (queue.Any()) {
                var node = queue.Dequeue();
                nodeDegree.Remove(node);

                foreach (var child in connections.Where(x => x.Source.Parent == node).Select(x => x.Target.Parent)) {
                    nodeDegree[child]--;

                    if (nodeDegree[child] == 0) {
                        queue.Enqueue(child);
                    }
                }
            }

            return nodeDegree.Any();
        }

        private void Connections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            if (e.OldItems != null) {
                foreach (ProcessConnect conn in e.OldItems) {
                    var localConn = Connections.FirstOrDefault(x => x.Source.Parent.SourceNode.Uid == conn.Previous &&
                                                                    x.Source.Id == conn.PreviousPort &&
                                                                    x.Target.Parent.SourceNode.Uid == conn.Next &&
                                                                    x.Target.Id == conn.NextPort);

                    if (localConn != null) {
                        Connections.Remove(localConn);
                    }
                }
            }

            if (e.NewItems != null) {
                foreach (ProcessConnect conn in e.NewItems) {
                    var startConn = Nodes.FirstOrDefault(x => x.SourceNode.Uid == conn.Previous)?
                        .Outputs.FirstOrDefault(x => x.Id == conn.PreviousPort);
                    var endConn = Nodes.FirstOrDefault(x => x.SourceNode.Uid == conn.Next)?
                        .Inputs.FirstOrDefault(x => x.Id == conn.NextPort);

                    if (startConn != null && endConn != null) {
                        Connections.Add(new Connection(startConn, endConn, startConn.Type));
                    }
                }
            }
        }

        private void Nodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            if (e.OldItems != null) {
                foreach (Node node in e.OldItems) {
                    var localNode = Nodes.FirstOrDefault(x => x.SourceNode == node);
                    if (localNode != null) {
                        Nodes.Remove(localNode);
                        localNode.Dispose();
                    }
                }
            }

            if (e.NewItems != null) {
                foreach (Node node in e.NewItems) {
                    Nodes.Add(node.GenerateNode());
                }
            }
        }

        public void RebuildGraph() {
            Nodes.Clear();
            Connections.Clear();


            var dict = new Dictionary<string, EditorNode>();
            foreach (var step in Graph.Nodes) {
                var node = step.GenerateNode();
                Nodes.Add(node);
                dict[step.Uid] = node;
                Tree[node] = new();
            }

            foreach (var con in Graph.ProcessConnects) {
                var start = dict[con.Previous].Outputs.FirstOrDefault(x => x.Id == con.PreviousPort);
                var finish = dict[con.Next].Inputs.FirstOrDefault(x => x.Id == con.NextPort);
                Connections.Add(new Connection(start, finish, start.Type));

                Tree[dict[con.Previous]].Add(dict[con.Next]);
            }
        }

        public void FitAll() {
            Editor.UpdateLayout();
            Editor.FitToScreen();
        }

        public void BringIntoView(EditorNode node) {
            Editor.UpdateLayout();

            var nodeContainer = Editor.Template.FindName("PART_ItemsHost", Editor) as NodifyCanvas;
            var nodeHost = nodeContainer.Children.OfType<ItemContainer>().FirstOrDefault(x => x.DataContext == node);

            Editor.BringIntoView(new Point(nodeHost.Location.X + nodeHost.RenderSize.Width / 2,
                nodeHost.Location.Y + nodeHost.RenderSize.Height / 2));
        }
    }
}