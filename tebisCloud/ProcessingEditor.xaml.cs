using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Microsoft.Xaml.Behaviors.Core;
using Nodify;
using tebisCloud.Data;
using tebisCloud.Data.Processing;
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
        public static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
            nameof(Nodes), typeof(ObservableCollection<EditorNode>), typeof(ProcessingEditor),
            new PropertyMetadata(default(ObservableCollection<EditorNode>)));

        public ObservableCollection<EditorNode> Nodes {
            get { return (ObservableCollection<EditorNode>)GetValue(NodesProperty); }
            set { SetValue(NodesProperty, value); }
        }

        public static readonly DependencyProperty ConnectionsProperty = DependencyProperty.Register(
            nameof(Connections), typeof(ObservableCollection<Connection>), typeof(ProcessingEditor),
            new PropertyMetadata(default(ObservableCollection<Connection>)));

        public ObservableCollection<Connection> Connections {
            get { return (ObservableCollection<Connection>)GetValue(ConnectionsProperty); }
            set { SetValue(ConnectionsProperty, value); }
        }

        public static readonly DependencyProperty PendingConnectionProperty = DependencyProperty.Register(
            nameof(PendingConnection), typeof(PendingConnection), typeof(ProcessingEditor),
            new PropertyMetadata(default(PendingConnection)));

        public PendingConnection PendingConnection {
            get { return (PendingConnection)GetValue(PendingConnectionProperty); }
            set { SetValue(PendingConnectionProperty, value); }
        }

        private ProcessingGraph _graph;

        public ICommand DisconnectCommand { get; }

        public static RoutedUICommand CreateNode { get; } = new();
        public ICommand AddNode { get; }

        public ICommand DeleteNode { get; }

        public ProcessingEditor() {
            Nodes = new();
            Connections = new();
            PendingConnection = new PendingConnection((a, b) => {
                if (a.Parameter != null) {
                    (a, b) = (b, a);
                }

                if (a.Type == b.Type) {
                    if (a.Parameter == null && b.Result == null &&
                        !Connections.Any(x => (x.Target == b && x.Source == a) || x.Target == b)) {
                        var con = new Connection(a, b, a.Type);
                        if (!IsCyclic(con)) {
                            Connections.Add(con);
                            _graph.ProcessConnects.Add(new() {
                                Previous = a.Parent.Step.Id,
                                PreviousPort = a.Id,
                                Next = b.Parent.Step.Id,
                                NextPort = b.Id,
                            });
                            Tree[a.Parent].Add(b.Parent);
                        }
                    }
                }
            });

            DisconnectCommand = new ActionCommand(x => {
                if (x is Connector point) {
                    var conn = Connections.FirstOrDefault(x => x.Target == point || x.Source == point);

                    if (conn != null) {
                        conn.Source.IsConnected = false;
                        conn.Target.IsConnected = false;
                        Connections.Remove(conn);

                        var dataConn = _graph.ProcessConnects.FirstOrDefault(x =>
                            x.Previous == conn.Source.Parent.Step.Id &&
                            x.PreviousPort == conn.Source.Id &&
                            x.Next == conn.Target.Parent.Step.Id &&
                            x.NextPort == conn.Target.Id
                        );

                        _graph.ProcessConnects.Remove(dataConn);
                        Tree[conn.Source.Parent].Remove(conn.Target.Parent);
                    }
                }
            });

            AddNode = new ActionCommand(() => {
                ContextMenu.PlacementTarget = this;
                ContextMenu.IsOpen = true;
            });

            DeleteNode = new ActionCommand(() => {
                if (((MultiSelector)Editor).SelectedItems.Count > 0) {
                    if (MessageBox.ShowDialog(this, "Sollen die ausgewählten Aktionen gelöscht werden?",
                            "Aktionen löschen", MessageBoxButton.YesNo) == true) {
                        DeleteSelectedNodes();
                    }
                }
            });

            InitializeComponent();

            SetProcessData(new());
        }

        private Dictionary<EditorNode, HashSet<EditorNode>> Tree { get; } = new();

        private bool IsCyclic(Connection? pending = null) {
            foreach (var node in Nodes.Where(x => !Connections.Any(y => y.Target.Parent == x))) {
                var visitMap = new List<EditorNode>();
                visitMap.Add(node);

                Queue<EditorNode> openNodes = new(Tree[node]);

                while (openNodes.Any()) {
                    var child = openNodes.Dequeue();

                    if (visitMap.Contains(child)) {
                        return true;
                    } else {
                        visitMap.Add(child);
                        foreach (var link in Tree[child]) {
                            openNodes.Enqueue(link);
                        }
                    }
                }

                if (pending != null) {
                    if (visitMap.Contains(pending.Source.Parent) && visitMap.Contains(pending.Target.Parent)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SetProcessData(ProcessingGraph graph) {
            _graph = graph;

            Nodes.Clear();
            Connections.Clear();


            var dict = new Dictionary<string, EditorNode>();
            foreach (var step in graph.Nodes) {
                var node = step.GenerateNode();
                Nodes.Add(node);
                dict[step.Id] = node;
                Tree[node] = new();
            }

            foreach (var con in graph.ProcessConnects) {
                var start = dict[con.Previous].Outputs.FirstOrDefault(x => x.Id == con.PreviousPort);
                var finish = dict[con.Next].Outputs.FirstOrDefault(x => x.Id == con.NextPort);
                Connections.Add(new Connection(start, finish, start.Type));

                Tree[dict[con.Previous]].Add(dict[con.Next]);
            }
        }

        private void CreateNode_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is Type t) {
                if (t.IsAssignableTo(typeof(Node))) {
                    if (Activator.CreateInstance(t) is Node step) {
                        Editor.ViewportTransform.Inverse.TryTransform(Mouse.GetPosition(Editor), out var point);
                        step.NodeLocation = point;
                        _graph.Nodes.Add(step);
                        var node = step.GenerateNode();
                        Nodes.Add(node);
                        Tree[node] = new();
                    }
                }
            }
        }

        private void DeleteSelectedNodes() {
            var selection = new List<EditorNode>();

            foreach (EditorNode node in ((MultiSelector)Editor).SelectedItems) {
                selection.Add(node);
            }

            foreach (var node in selection) {
                Nodes.Remove(node);
                Tree.Remove(node);

                var conns = Connections.Where(x => x.Source.Parent == node || x.Target.Parent == node).ToList();

                foreach (var con in conns) {
                    Connections.Remove(con);
                }
            }

            Editor.SelectedItem = null;
        }
    }
}