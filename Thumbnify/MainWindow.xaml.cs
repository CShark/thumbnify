using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using FlyleafLib.MediaPlayer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using Config = FlyleafLib.Config;
using Path = System.IO.Path;
using Newtonsoft.Json;
using Thumbnify.Controls;
using Thumbnify.Data;
using Thumbnify.Data.Processing;
using Thumbnify.Dialogs;
using Thumbnify.NAudio;
using Thumbnify.Tools;
using MessageBox = Thumbnify.Dialogs.MessageBox;

namespace Thumbnify {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private const long MaxZoom = 8000000;
        private const long MinZoom = 400000;
        private readonly long MediaPartBorder = TimeSpan.FromSeconds(15).Ticks;

        public static RoutedUICommand EditMediaPart { get; } = new();
        public static RoutedUICommand DeleteMediaPart { get; } = new();

        public static RoutedUICommand DeleteMedia { get; } = new();
        public static RoutedUICommand AddToQueue { get; } = new();
        public static RoutedUICommand DelFromQueue { get; } = new();
        public static RoutedUICommand SelectMediaPart { get; } = new();
        public static RoutedUICommand PlayPauseMedia { get; } = new();

        public static RoutedUICommand SetIn { get; } = new();
        public static RoutedUICommand SetOut { get; } = new();
        public static RoutedUICommand ClearIn { get; } = new();
        public static RoutedUICommand ClearOut { get; } = new();
        public static RoutedUICommand ClearInOut { get; } = new();

        public static RoutedUICommand CreateMediaPart { get; } = new();

        public static RoutedUICommand NextMediaPart { get; } = new();
        public static RoutedUICommand NextMediaItem { get; } = new();
        public static RoutedUICommand PrevMediaPart { get; } = new();
        public static RoutedUICommand PrevMediaItem { get; } = new();

        public static RoutedUICommand MovePartForward { get; } = new();
        public static RoutedUICommand MovePartForwardFine { get; } = new();
        public static RoutedUICommand MovePartBackward { get; } = new();
        public static RoutedUICommand MovePartBackwardFine { get; } = new();


        public List<Color> PartColors = new() {
            Colors.DarkOrchid,
            Colors.OrangeRed,
            Colors.OliveDrab
        };

        public Player Player { get; set; }
        public Config Config { get; set; }
        public Player InPlayer { get; set; }
        public Player OutPlayer { get; set; }

        private Timer _frameOverlayTimer;

        public ICollectionView Media { get; set; }

        public static readonly DependencyProperty SelectedMediaProperty = DependencyProperty.Register(
            nameof(SelectedMedia), typeof(MediaSource), typeof(MainWindow),
            new PropertyMetadata(default(MediaSource?)));

        public MediaSource? SelectedMedia {
            get { return (MediaSource?)GetValue(SelectedMediaProperty); }
            set { SetValue(SelectedMediaProperty, value); }
        }

        public static readonly DependencyProperty DetailWaveformZoomProperty = DependencyProperty.Register(
            nameof(DetailWaveformZoom), typeof(long), typeof(MainWindow), new PropertyMetadata((long)1000000));

        public long DetailWaveformZoom {
            get { return (long)GetValue(DetailWaveformZoomProperty); }
            set { SetValue(DetailWaveformZoomProperty, value); }
        }

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            nameof(IsPlaying), typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

        public bool IsPlaying {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public static readonly DependencyProperty SelectionVisibleProperty = DependencyProperty.Register(
            nameof(SelectionVisible), typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

        public bool SelectionVisible {
            get { return (bool)GetValue(SelectionVisibleProperty); }
            set { SetValue(SelectionVisibleProperty, value); }
        }

        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
            nameof(SelectionStart), typeof(long), typeof(MainWindow), new PropertyMetadata(default(long)));

        public long SelectionStart {
            get { return (long)GetValue(SelectionStartProperty); }
            set { SetValue(SelectionStartProperty, value); }
        }

        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(
            nameof(SelectionEnd), typeof(long), typeof(MainWindow), new PropertyMetadata(default(long)));

        public long SelectionEnd {
            get { return (long)GetValue(SelectionEndProperty); }
            set { SetValue(SelectionEndProperty, value); }
        }

        public static readonly DependencyProperty SelectionLengthProperty = DependencyProperty.Register(
            nameof(SelectionLength), typeof(long), typeof(MainWindow), new PropertyMetadata(default(long)));

        public long SelectionLength {
            get { return (long)GetValue(SelectionLengthProperty); }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public static readonly DependencyProperty UploadQueueProperty = DependencyProperty.Register(
            nameof(UploadQueue), typeof(ObservableCollection<QueueItemStatus>), typeof(MainWindow),
            new PropertyMetadata(default(ObservableCollection<QueueItemStatus>)));

        public ObservableCollection<QueueItemStatus> UploadQueue {
            get { return (ObservableCollection<QueueItemStatus>)GetValue(UploadQueueProperty); }
            set { SetValue(UploadQueueProperty, value); }
        }

        public static readonly DependencyProperty SelectedMediaPartProperty = DependencyProperty.Register(
            nameof(SelectedMediaPart), typeof(MediaPart), typeof(MainWindow),
            new FrameworkPropertyMetadata(default(MediaPart?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, args) => ((MainWindow)o).UpdateSelectedPartShortcuts()));

        public MediaPart? SelectedMediaPart {
            get { return (MediaPart?)GetValue(SelectedMediaPartProperty); }
            set { SetValue(SelectedMediaPartProperty, value); }
        }

        public static readonly DependencyProperty PartSelectionModeProperty = DependencyProperty.Register(
            nameof(PartSelectionMode), typeof(PartSelectionMode), typeof(MainWindow),
            new PropertyMetadata(default(PartSelectionMode)));

        public PartSelectionMode PartSelectionMode {
            get { return (PartSelectionMode)GetValue(PartSelectionModeProperty); }
            set { SetValue(PartSelectionModeProperty, value); }
        }

        public static readonly DependencyProperty ShowInOutOverlayProperty = DependencyProperty.Register(
            nameof(ShowInOutOverlay), typeof(bool), typeof(MainWindow), new PropertyMetadata(default(bool)));

        public bool ShowInOutOverlay {
            get { return (bool)GetValue(ShowInOutOverlayProperty); }
            set { SetValue(ShowInOutOverlayProperty, value); }
        }

        #region ShortcutInfo

        public static readonly DependencyProperty ShortcutsProperty = DependencyProperty.Register(
            nameof(Shortcuts), typeof(ObservableCollection<ShortcutData>), typeof(MainWindow),
            new PropertyMetadata(default(ObservableCollection<ShortcutData>)));

        public ObservableCollection<ShortcutData> Shortcuts {
            get { return (ObservableCollection<ShortcutData>)GetValue(ShortcutsProperty); }
            set { SetValue(ShortcutsProperty, value); }
        }

        private readonly ObservableCollection<ShortcutData> _defaultShortcuts = new() {
            new("TimelinePlay", new Shortcut("space")),
            new("TimelineMute", new Shortcut("M")),
            new("TimelineBackward", new Shortcut("Ctrl", "Shift", "\u2190"), new Shortcut("Shift", "\u2190"),
                new Shortcut("\u2190")),
            new("TimelineForward", new Shortcut("Ctrl", "Shift", "\u2192"), new Shortcut("Shift", "\u2192"),
                new Shortcut("\u2192")),
            new("TimelinePrevFrame", new Shortcut(",")),
            new("TimelineNextFrame", new Shortcut(".")),
        };

        private readonly ObservableCollection<ShortcutData> _mediaShortcuts = new() {
            new("TimelinePrevPart", new Shortcut("\u2191")),
            new("TimelineNextPart", new Shortcut("\u2193")),
            new("TimelinePrevMedia", new Shortcut("Ctrl", "Shift", "\u2191")),
            new("TimelineNextMedia", new Shortcut("Ctrl", "Shift", "\u2193"))
        };

        private readonly ObservableCollection<ShortcutData> _timelineShortcuts = new() {
            new("TimelineDrag", new Shortcut(MouseButton.Left)),
            new("TimelineIn", new Shortcut("I")),
            new("TimelineOut", new Shortcut("O"))
        };

        #endregion

        public MainWindow() {
            Config = new Config();
            Config.Video.BackgroundColor = Color.FromRgb(0x31, 0x31, 0x31);
            Config.Player.AutoPlay = false;
            Config.Player.SeekAccurate = true;
            Config.Player.UICurTimePerFrame = true;
            Config.Player.VolumeMax = 300;
            Config.Player.SeekOffset = TimeSpan.FromSeconds(.5).Ticks;
            Config.Player.SeekOffset2 = TimeSpan.FromSeconds(5).Ticks;
            Config.Player.SeekOffset3 = TimeSpan.FromMinutes(1).Ticks;

            Player = new Player(Config);
            InPlayer = new Player(new Config {
                Player = { SeekAccurate = true, AutoPlay = false, UICurTimePerFrame = true },
                Video = { BackgroundColor = Color.FromRgb(0x31, 0x31, 0x31) }
            });
            OutPlayer = new Player(new Config {
                Player = { SeekAccurate = true, AutoPlay = false, UICurTimePerFrame = true },
                Video = { BackgroundColor = Color.FromRgb(0x31, 0x31, 0x31) }
            });

            Media = new ListCollectionView(App.Settings.Media);
            Media.SortDescriptions.Add(new SortDescription(nameof(MediaSource.Date), ListSortDirection.Descending));

            UploadQueue = new();
            Shortcuts = _defaultShortcuts;

            ScanMedia();

            InitializeCommandBindings();
            InitializeComponent();
            
            Media.MoveCurrentToFirst();
            MediaList_OnSelectionChanged(null, null);

            CommandManager.InvalidateRequerySuggested();

            VideoSeekSlider.PreviewMouseMove += (sender, args) => {
                if (args.LeftButton == MouseButtonState.Pressed) {
                    VideoSeekSlider.RaiseEvent(
                        new MouseButtonEventArgs(args.MouseDevice, args.Timestamp, MouseButton.Left) {
                            RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                            Source = args.Source
                        });
                }
            };

            PartSelectionMode = PartSelectionMode.All;

            Player.PropertyChanged += (sender, args) => {
                if (args.PropertyName == nameof(Player.CurTime)) {
                    UpdatePartSelection();
                }
            };

            _frameOverlayTimer = new Timer(_ => {
                try {
                    Dispatcher.Invoke(() => { ShowInOutOverlay = false; });
                } catch {
                    // ignored
                }
            });

            foreach (var item in App.Settings.StaticGraphs) {
                var graph = App.Settings.Processing.FirstOrDefault(x => x.Name == item.GraphName);

                if (graph != null) {
                    UploadQueue.Add(new QueueItemStatus(graph) { AutomaticallyCreated = true });
                }
            }
        }

        private void OpenSettings_OnClick(object sender, RoutedEventArgs e) {
            var settings = new Settings(App.Settings);
            settings.Owner = this;
            settings.ShowDialog();

            App.SaveSettings();
            ScanMedia();

            SelectedMedia = App.Settings.Media.MaxBy(x => x.Date);

            var autoItems = UploadQueue.Where(x => x.AutomaticallyCreated && x.MediaPart == null).ToList();
            foreach (var item in autoItems) {
                UploadQueue.Remove(item);
            }

            foreach (var item in App.Settings.StaticGraphs.Reverse()) {
                var graph = App.Settings.Processing.FirstOrDefault(x => x.Name == item.GraphName);

                if (graph != null) {
                    UploadQueue.Insert(0, new QueueItemStatus(graph) { AutomaticallyCreated = true });
                }
            }
        }

        private void ScanMedia() {
            if (Directory.Exists(App.Settings.VideoPath)) {
                var files = Directory.EnumerateFiles(App.Settings.VideoPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(
                        x => x.ToLower().EndsWith(".mkv") || x.ToLower().EndsWith(".mp4"));

                foreach (var media in App.Settings.Media) {
                    media.FileExists = false;
                }

                foreach (var file in files) {
                    var media = App.Settings.Media.FirstOrDefault(x => x.FileName == file);

                    if (media != null) {
                        media.FileExists = true;
                    } else {
                        media = new MediaSource {
                            FileName = file,
                            Name = Path.GetFileNameWithoutExtension(file),
                            Date = File.GetCreationTime(file),
                            FileExists = true
                        };
                        App.Settings.Media.Add(media);
                    }

                    if (media.Parts.Any() && media.Parts.All(x => x.ProcessingCompleted)) {
                        media.SlatedForCleanup = true;
                    } else {
                        media.SlatedForCleanup = false;
                    }
                }

                var toDelete = App.Settings.Media.Where(x => !x.FileExists).ToList();
                foreach (var media in toDelete) {
                    App.Settings.Media.Remove(media);
                }
            }

            App.SaveSettings();
        }

        private void MediaList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = Media.CurrentItem as MediaSource;
            SelectedMedia = item;

            Player.Stop();
            IsPlaying = false;

            if (item != null) {
                if (item.UiData.FileStream == null) {
                    item.UiData.FileStream = new FileStream(item.FileName, FileMode.Open,
                        FileAccess.Read, FileShare.Read);
                    item.UiData.FileStreamIn = new FileStream(item.FileName, FileMode.Open,
                        FileAccess.Read, FileShare.Read);
                    item.UiData.FileStreamOut = new FileStream(item.FileName, FileMode.Open,
                        FileAccess.Read, FileShare.Read);
                }


                Player.Open(item.UiData.FileStream);
                InPlayer.Open(item.UiData.FileStreamIn);
                OutPlayer.Open(item.UiData.FileStreamOut);
                Player.ShowFrame(0);
                InPlayer.ShowFrame(0);
                OutPlayer.ShowFrame(0);

                if (item.UiData.AudioEngine == null) {
                    item.UiData.AudioEngine = new();
                    item.UiData.AudioEngine.OpenFile(item.FileName);
                }

                waveformOverview?.RegisterSoundPlayer(item.UiData.AudioEngine);
                waveformDetail?.RegisterSoundPlayer(item.UiData.AudioEngine);
            } else {
                var tmp = new NAudioEngine();
                waveformOverview?.RegisterSoundPlayer(tmp);
                waveformDetail?.RegisterSoundPlayer(tmp);
            }

            SelectionVisible = false;
            SelectionStart = 0;
            SelectionEnd = 0;

            if (waveformOverview != null) {
                var brush = waveformOverview.LeftLevelBrush;
                waveformOverview.LeftLevelBrush = new SolidColorBrush(Colors.Black);
                waveformOverview.LeftLevelBrush = brush;
            }
        }

        private void StopButton_OnClick(object sender, RoutedEventArgs e) {
            Player.Pause();
            Player.ShowFrame(0);
            IsPlaying = false;
        }

        private void PlayButton_OnClick(object sender, RoutedEventArgs e) {
            Player.TogglePlayPause();
            IsPlaying = !IsPlaying;
        }

        private void VolumeSlider_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Player.Audio.Volume = 100;
        }

        #region Waveform dragging

        private bool _isWaveformDragging = false;
        private Point _waveformDragStart = new();
        private bool _dragMediaPart = false;

        private void DragWaveform_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _isWaveformDragging = true;
            _waveformDragStart = e.GetPosition(this);
            _dragMediaPart = e.RightButton == MouseButtonState.Pressed;

            if (_dragMediaPart) {
                MoveMediaPart(0);
            }
        }

        private void DragWaveform_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            _isWaveformDragging = false;
        }

        private void DragWaveform_OnMouseMove(object sender, MouseEventArgs e) {
            if (_isWaveformDragging) {
                var offset = (long)((_waveformDragStart.X - e.GetPosition(this).X) * DetailWaveformZoom);

                if (offset != 0) {
                    if (_dragMediaPart) {
                        MoveMediaPart(offset);
                    } else {
                        Player.CurTime = Math.Clamp(Player.CurTime + offset, 0, Player.Duration);
                    }
                }

                _waveformDragStart = e.GetPosition(this);
            } else {
                FrameworkElement? element = null;

                VisualTreeHelper.HitTest(this, null, x => {
                    if (x.VisualHit is FrameworkElement elem) {
                        if (elem.DataContext is MediaPart media &&
                            VisualTreeExtensions.HasParentOfType<Viewbox>(elem)) {
                            SelectedMediaPart = media;
                            element = elem;
                            PartSelectionMode = PartSelectionMode.All;
                            return HitTestResultBehavior.Stop;
                        }
                    }

                    return HitTestResultBehavior.Continue;
                }, new PointHitTestParameters(e.GetPosition(this)));

                if (element != null) {
                    var transform = element.TransformToAncestor(this);
                    var pos = e.GetPosition(this);

                    var start = transform.Transform(new Point(MediaPartBorder, 0));
                    var end = transform.Transform(new Point(SelectedMediaPart.Duration - MediaPartBorder, 0));

                    if (start.X > pos.X) {
                        PartSelectionMode = PartSelectionMode.Start;
                    } else if (end.X < pos.X) {
                        PartSelectionMode = PartSelectionMode.End;
                    }
                } else {
                    SelectedMediaPart = null;
                }
            }
        }

        private void DragWaveform_OnMouseLeave(object sender, MouseEventArgs e) {
            _isWaveformDragging = false;
        }

        #endregion

        private void ZoomReset_OnClick(object sender, RoutedEventArgs e) {
            DetailWaveformZoom = 1000000;
        }

        private void ZoomOut_OnClick(object sender, RoutedEventArgs e) {
            if (DetailWaveformZoom < MaxZoom) {
                DetailWaveformZoom *= 2;
            }
        }

        private void ZoomIn_OnClick(object sender, RoutedEventArgs e) {
            if (DetailWaveformZoom > MinZoom) {
                DetailWaveformZoom /= 2;
            }
        }

        private void PartAreaOverlay_OnPartClicked(MediaPart obj) {
            Player.CurTime = obj.Start;
        }

        private void MediaSelected_CanExecetute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = SelectedMedia != null;
        }

        private void EditMediaPart_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = e.Parameter is MediaPart;
        }

        private void EditMediaPart_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is MediaPart part) {
                var json = JsonConvert.SerializeObject(part);

                var dlg = new EditPartMetadata(part.Metadata);
                dlg.Owner = this;

                if (dlg.ShowDialog() != true) {
                    part.Metadata = null;
                    JsonConvert.PopulateObject(json, part);
                } else {
                    App.SaveSettings();
                }
            }
        }

        private void DeleteMediaPart_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is MediaPart part) {
                if (MessageBox.ShowDialog(this, "deleteMediaPart", MessageBoxButton.YesNo) == true) {
                    var queueItem = UploadQueue.FirstOrDefault(x => x.MediaPart == part);
                    part.Parent.Parts.Remove(part);
                    UploadQueue.Remove(queueItem);
                }
            }
        }

        private void EditThumbnails_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ThumbnailPresetEditor();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void EditPostprocessing_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ProcessingEditor();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void StartProcessing_OnClick(object sender, RoutedEventArgs e) {
            // Update queue items with graphs from media part
            foreach (var item in UploadQueue) {
                if (item.MediaPart != null) {
                    item.Graph = item.MediaPart.Metadata.ProcessingGraph;
                }
            }
            
            var dlg = new ProcessingStatus();
            dlg.Owner = this;
            dlg.StartProcessing(UploadQueue);
            dlg.ShowDialog();

            var done = UploadQueue.Where(x => x.Graph.GraphState == ENodeStatus.Completed).ToList();
            foreach (var item in done) {
                UploadQueue.Remove(item);
            }
        }

        private void InitializeCommandBindings() {
            #region Queue Commands

            CommandBindings.Add(new CommandBinding(AddToQueue, (_, args) => {
                if (args.Parameter is MediaPart part) {
                    UploadQueue.Add(new(part));
                }
            }, (_, args) => {
                var param = args.Parameter as MediaPart;
                args.CanExecute = false;

                if (param != null && !UploadQueue.Any(x => x.MediaPart == param)) {
                    args.CanExecute = true;
                }
            }));

            CommandBindings.Add(new(DelFromQueue, (_, args) => {
                if (args.Parameter is QueueItemStatus queueItem) {
                    UploadQueue.Remove(queueItem);
                }
            }, (_, args) => {
                var param = args.Parameter as QueueItemStatus;
                args.CanExecute = false;

                if (param != null && UploadQueue.Contains(param)) {
                    args.CanExecute = true;
                }
            }));

            #endregion

            #region Selection Commands

            CommandBindings.Add(new(SelectMediaPart, (_, args) => {
                if (args.Parameter is MediaPart part) {
                    SelectedMedia = part.Parent;
                    Player.CurTime = part.Start;
                }
            }, MediaSelected_CanExecetute));

            CommandBindings.Add(new(SetIn, (_, _) => {
                if (SelectedMedia == null) return;

                SelectionStart = Player.CurTime;
                if (!SelectionVisible) {
                    SelectionEnd = Player.Duration;
                }

                SelectionVisible = true;

                if (SelectionEnd < SelectionStart) {
                    (SelectionStart, SelectionEnd) = (SelectionEnd, SelectionStart);
                }

                SelectionLength = SelectionEnd - SelectionStart;
                UpdateInOutShortcuts();
            }, MediaSelected_CanExecetute));

            CommandBindings.Add(new(SetOut, (_, _) => {
                if (SelectedMedia == null) return;

                SelectionEnd = Player.CurTime;
                if (!SelectionVisible) {
                    SelectionStart = 0;
                }

                SelectionVisible = true;

                if (SelectionEnd < SelectionStart) {
                    (SelectionStart, SelectionEnd) = (SelectionEnd, SelectionStart);
                }

                SelectionLength = SelectionEnd - SelectionStart;
                UpdateInOutShortcuts();
            }, MediaSelected_CanExecetute));

            CommandBindings.Add(new(ClearIn, (_, _) => {
                if (SelectedMedia == null) return;

                SelectionStart = 0;

                if (SelectionStart == 0 && SelectionEnd == Player.Duration) {
                    SelectionVisible = false;
                }

                SelectionLength = SelectionEnd - SelectionStart;
                UpdateInOutShortcuts();
            }, MediaSelected_CanExecetute));

            CommandBindings.Add(new(ClearOut, (_, _) => {
                if (SelectedMedia == null) return;

                SelectionEnd = Player.Duration;

                if (SelectionStart == 0 && SelectionEnd == Player.Duration) {
                    SelectionVisible = false;
                }

                SelectionLength = SelectionEnd - SelectionStart;
                UpdateInOutShortcuts();
            }, MediaSelected_CanExecetute));

            CommandBindings.Add(new(ClearInOut, (_, _) => {
                if (SelectedMedia == null) return;

                SelectionStart = 0;
                SelectionEnd = Player.Duration;
                SelectionVisible = false;
                SelectionLength = Player.Duration;
                UpdateInOutShortcuts();
            }, MediaSelected_CanExecetute));

            CommandBindings.Add(new(CreateMediaPart, (_, _) => { CreateMediaPartFromSelection(); },
                (_, args) => args.CanExecute = SelectionVisible));

            #endregion

            #region Media Navigation

            CommandBindings.Add(new(NextMediaItem, (_, _) => { Media.MoveCurrentToNext(); }));

            CommandBindings.Add(new(PrevMediaItem, (_, _) => { Media.MoveCurrentToPrevious(); }));

            CommandBindings.Add(new(NextMediaPart, (_, _) => {
                var next = GetPartPositions().OrderBy(x => x.Position).FirstOrDefault(x => x.Position > Player.CurTime);

                if (next.Part != null) {
                    Player.CurTime = next.Position;
                } else {
                    Player.CurTime = Player.Duration;
                }
            }));

            CommandBindings.Add(new(PrevMediaPart, (_, _) => {
                var prev = GetPartPositions().OrderByDescending(x => x.Position)
                    .FirstOrDefault(x => x.Position < Player.CurTime);

                if (prev.Part != null) {
                    Player.CurTime = prev.Position;
                } else {
                    Player.CurTime = 0;
                }
            }));

            #endregion

            #region Part Movement

            CommandBindings.Add(new(MovePartForward, (_, _) => { MoveMediaPart(Config.Player.SeekOffset2); },
                (_, args) => { args.CanExecute = SelectedMediaPart != null; }));

            CommandBindings.Add(new(MovePartForwardFine, (_, _) => { MoveMediaPart(Config.Player.SeekOffset); },
                (_, args) => { args.CanExecute = SelectedMediaPart != null; }));

            CommandBindings.Add(new(MovePartBackward, (_, _) => { MoveMediaPart(-Config.Player.SeekOffset2); },
                (_, args) => { args.CanExecute = SelectedMediaPart != null; }));

            CommandBindings.Add(new(MovePartBackwardFine, (_, _) => { MoveMediaPart(-Config.Player.SeekOffset); },
                (_, args) => { args.CanExecute = SelectedMediaPart != null; }));

            #endregion

            CommandBindings.Add(new(PlayPauseMedia, (_, _) => { PlayButton_OnClick(null, null); },
                (_, args) => args.CanExecute = SelectedMedia != null));

            CommandBindings.Add(new(DeleteMedia, (_, e) => {
                if (e.Parameter is MediaSource media) {
                    if (MessageBox.ShowDialog(this, "deleteMedia", MessageBoxButton.YesNo) == true) {
                        DeleteMediaFile(media);
                        App.SaveSettings();
                    }
                }
            }, (_, e) => { e.CanExecute = e.Parameter is MediaSource; }));

            PreviewKeyDown += (sender, args) => {
                if (args.Key == Key.Enter) {
                    args.Handled = true;

                    if (args.IsDown) {
                        CreateMediaPart.Execute(null, this);
                    }
                }
            };

            IEnumerable<(long Position, MediaPart Part)> GetPartPositions() =>
                SelectedMedia?.Parts.Select(x => (x.Start, x))?.Concat(SelectedMedia.Parts.Select(x => (x.End, x))) ??
                Array.Empty<(long, MediaPart)>();
        }

        private void MoveMediaPart(long offset) {
            if (SelectedMediaPart == null) return;

            if (PartSelectionMode == PartSelectionMode.Start) {
                Player.CurTime = SelectedMediaPart.Start;
            } else if (PartSelectionMode == PartSelectionMode.End) {
                Player.CurTime = SelectedMediaPart.End;
            } else if (PartSelectionMode == PartSelectionMode.All) {
                if (Player.CurTime < SelectedMediaPart.Start || Player.CurTime > SelectedMediaPart.End) {
                    Player.CurTime = SelectedMediaPart.Start + SelectedMediaPart.Duration / 2;
                }
            }

            var newTime = Math.Clamp(Player.CurTime + offset, 0, Player.Duration);
            offset = newTime - Player.CurTime;
            Player.CurTime = newTime;

            switch (PartSelectionMode) {
                case PartSelectionMode.Start:
                    if (offset < 0 || SelectedMediaPart.Duration > Math.Min(offset, TimeSpan.FromSeconds(5).Ticks)) {
                        SelectedMediaPart.Start = Player.CurTime;
                        SelectedMediaPart.Duration = SelectedMediaPart.End - SelectedMediaPart.Start;
                    }

                    break;
                case PartSelectionMode.End:
                    if (offset > 0 || SelectedMediaPart.Duration > -Math.Min(offset, TimeSpan.FromSeconds(5).Ticks)) {
                        SelectedMediaPart.End = Player.CurTime;
                        SelectedMediaPart.Duration = SelectedMediaPart.End - SelectedMediaPart.Start;
                    }

                    break;
                case PartSelectionMode.All:
                    SelectedMediaPart.Start = Math.Clamp(SelectedMediaPart.Start + offset, 0,
                        Player.Duration - SelectedMediaPart.Duration);
                    SelectedMediaPart.End = SelectedMediaPart.Start + SelectedMediaPart.Duration;

                    InPlayer.CurTime = SelectedMediaPart.Start;
                    OutPlayer.CurTime = SelectedMediaPart.End;
                    ShowInOutOverlay = true;
                    _frameOverlayTimer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);

                    break;
            }
        }

        private void CreateMediaPartFromSelection() {
            if (!SelectionVisible) return;

            SelectionVisible = false;

            if (SelectedMedia != null) {
                var graph = App.Settings.Processing.FirstOrDefault(x =>
                    x.Name.ToLower() == App.Settings.DefaultProcessing.ToLower());

                if (graph != null) {
                    var json = JsonConvert.SerializeObject(graph);
                    graph = JsonConvert.DeserializeObject<ProcessingGraph>(json);
                }

                var part = new MediaPart {
                    Start = SelectionStart,
                    End = SelectionEnd,
                    Duration = SelectionLength,
                    Name = $"#{SelectedMedia.Parts.Count}",
                    Metadata = new PartMetadata {
                        ProcessingGraph = graph,
                        Color = PartColors[SelectedMedia.Parts.Count % PartColors.Count],
                    },
                    Parent = SelectedMedia
                };

                part.Metadata.UpdateParameters();

                var dlg = new EditPartMetadata(part.Metadata);
                dlg.Owner = this;

                if (dlg.ShowDialog() == true) {
                    SelectedMedia.Parts.Add(part);
                    UploadQueue.Add(new(part));
                    App.SaveSettings();
                }
            }

            SelectionStart = 0;
            SelectionEnd = 0;
        }

        private void UpdatePartSelection() {
            if (Player.IsPlaying) return;
            if (SelectedMedia == null) return;
            if (!SelectedMedia.Parts.Any()) return;
            if (_isWaveformDragging && _dragMediaPart) return;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) return;

            var inside = SelectedMedia.Parts.LastOrDefault(x => x.Start < Player.CurTime && x.End > Player.CurTime);
            var closestStart = SelectedMedia.Parts.MinBy(x => Math.Abs(Player.CurTime - x.Start));
            var closestEnd = SelectedMedia.Parts.MinBy(x => Math.Abs(Player.CurTime - x.End));

            var distStart = Math.Abs(closestStart.Start - Player.CurTime);
            var distEnd = Math.Abs(closestEnd.End - Player.CurTime);

            if (distStart < distEnd) {
                if (distStart < MediaPartBorder) {
                    SelectedMediaPart = closestStart;
                    PartSelectionMode = PartSelectionMode.Start;
                    return;
                }
            } else {
                if (distEnd < MediaPartBorder) {
                    SelectedMediaPart = closestEnd;
                    PartSelectionMode = PartSelectionMode.End;
                    return;
                }
            }

            if (inside != null) {
                SelectedMediaPart = inside;
                PartSelectionMode = PartSelectionMode.All;
            } else {
                SelectedMediaPart = null;
                PartSelectionMode = PartSelectionMode.All;
            }
        }

        private void MainWindow_OnClosed(object? sender, EventArgs e) {
            App.SaveSettings();
        }

        private void Cleanup_OnClick(object sender, RoutedEventArgs e) {
            if (MessageBox.ShowDialog(this, "cleanup", MessageBoxButton.YesNo) == true) {
                var files = App.Settings.Media.Where(x => x.SlatedForCleanup).ToList();

                foreach (var file in files) {
                    DeleteMediaFile(file);
                }

                App.SaveSettings();
            }
        }

        private void DeleteMediaFile(MediaSource media) {
            try {
                media.UiData.AudioEngine?.Dispose();
                media.UiData.FileStream?.Dispose();
                media.UiData.FileStreamIn?.Dispose();
                media.UiData.FileStreamOut?.Dispose();
                File.Delete(media.FileName);
                File.Delete(media.FileName + ".peaks");
            } catch (Exception ex) {
                // ignored
            }

            if (!File.Exists(media.FileName) && !File.Exists(media.FileName + ".peaks")) {
                App.Settings.Media.Remove(media);
            }
        }

        private void LogViewer_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new GraphViewer();
            dlg.Owner = this;
            dlg.Show();
        }

        #region Shortcut Infobar

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e) {
            if (Shortcuts != _defaultShortcuts) {
                Shortcuts = _defaultShortcuts;
            }
        }

        private void MediaList_OnMouseMove(object sender, MouseEventArgs e) {
            e.Handled = true;
            if (Shortcuts != _mediaShortcuts) {
                Shortcuts = _mediaShortcuts;
            }
        }

        private void Timeline_OnMouseMove(object sender, MouseEventArgs e) {
            e.Handled = true;
            if (Shortcuts != _timelineShortcuts) {
                Shortcuts = _timelineShortcuts;
            }
        }

        private void UpdateSelectedPartShortcuts() {
            var dragPart = _timelineShortcuts.FirstOrDefault(x => x.Description == "TimelineItemDrag");
            var moveForward = _timelineShortcuts.FirstOrDefault(x => x.Description == "TimelineMoveForward");
            var moveBackward = _timelineShortcuts.FirstOrDefault(x => x.Description == "TimelineMoveBackward");

            if (SelectedMediaPart == null) {
                if (dragPart != null) {
                    _timelineShortcuts.Remove(dragPart);
                }

                if (moveForward != null) {
                    _timelineShortcuts.Remove(moveForward);
                }

                if (moveBackward != null) {
                    _timelineShortcuts.Remove(moveBackward);
                }
            } else {
                if (dragPart == null) {
                    _timelineShortcuts.Insert(1, new ShortcutData("TimelineItemDrag", new Shortcut(MouseButton.Right)));
                }

                if (moveBackward == null) {
                    _timelineShortcuts.Insert(2,
                        new ShortcutData("TimelineMoveBackward", new Shortcut("Alt", "Shift", "\u2190"),
                            new Shortcut("Alt", "\u2190")));
                }

                if (moveForward == null) {
                    _timelineShortcuts.Insert(3,
                        new ShortcutData("TimelineMoveForward", new Shortcut("Alt", "Shift", "\u2192"),
                            new Shortcut("Alt", "\u2192")));
                }
            }
        }

        private void UpdateInOutShortcuts() {
            var @in = _timelineShortcuts.FirstOrDefault(x => x.Description == "TimelineIn");
            var clearIn = _timelineShortcuts.FirstOrDefault(x => x.Description == "TimelineClearIn");
            var @out = _timelineShortcuts.FirstOrDefault(x => x.Description == "TimelineOut");
            var clearOut = _timelineShortcuts.FirstOrDefault(x => x.Description == "TimelineClearOut");
            var createPart = _timelineShortcuts.FirstOrDefault(x => x.Description == "TimelineCreatePart");

            if (SelectionVisible) {
                if (clearIn == null) {
                    _timelineShortcuts.Insert(_timelineShortcuts.IndexOf(@in) + 1,
                        new ShortcutData("TimelineClearIn", new Shortcut("Alt", "I")));
                }

                if (clearOut == null) {
                    _timelineShortcuts.Insert(_timelineShortcuts.IndexOf(@out) + 1,
                        new ShortcutData("TimelineClearOut", new Shortcut("Alt", "O")));
                }

                if (createPart == null) {
                    _timelineShortcuts.Insert(_timelineShortcuts.IndexOf(@out) + 2,
                        new ShortcutData("TimelineCreatePart", new Shortcut("Enter")));
                }
            } else {
                if (clearIn != null) {
                    _timelineShortcuts.Remove(clearIn);
                }

                if (clearOut != null) {
                    _timelineShortcuts.Remove(clearOut);
                }

                if (createPart != null) {
                    _timelineShortcuts.Remove(createPart);
                }
            }
        }

        #endregion

        private void AddStaticGraph_OnClick(object sender, RoutedEventArgs e) {
            var result = LoadSaveDialog.ShowOpenDialog(this, App.Settings.Processing.Where(x => !x.RequiresMediaPart));

            if (result != null) {
                UploadQueue.Add(new(result));
            }
        }

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                CreateMediaPart.Execute(null,this);
            }else if (e.Key == Key.Space) {
                e.Handled = true;
                Player.Commands.TogglePlayPause.Execute(null);
            }
        }
    }
}