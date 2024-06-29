using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using FlyleafLib.MediaPlayer;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using tebisCloud.Data;
using tebisCloud.NAudio;
using Config = FlyleafLib.Config;
using Path = System.IO.Path;
using FlyleafLib;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using tebisCloud.Dialogs;
using MessageBox = tebisCloud.Dialogs.MessageBox;

namespace tebisCloud {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static RoutedUICommand EditMediaPart { get; } = new();
        public static RoutedUICommand DeleteMediaPart { get; } = new();

        public List<Color> PartColors = new() {
            Colors.DarkOrchid,
            Colors.OrangeRed,
            Colors.OliveDrab
        };

        public Player Player { get; set; }
        public Config Config { get; set; }

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
            nameof(UploadQueue), typeof(ObservableCollection<MediaPart>), typeof(MainWindow),
            new PropertyMetadata(default(ObservableCollection<MediaPart>)));

        public ObservableCollection<MediaPart> UploadQueue {
            get { return (ObservableCollection<MediaPart>)GetValue(UploadQueueProperty); }
            set { SetValue(UploadQueueProperty, value); }
        }

        private const long MaxZoom = 2000000;
        private const long MinZoom = 400000;

        public MainWindow() {
            Config = new Config();
            Config.Video.BackgroundColor = Color.FromRgb(0x31, 0x31, 0x31);
            Config.Player.AutoPlay = false;
            Config.Player.SeekAccurate = true;
            Config.Player.UICurTimePerFrame = true;
            Config.Player.VolumeMax = 200;

            Player = new Player(Config);

            Media = new ListCollectionView(App.Settings.Media);
            Media.SortDescriptions.Add(new SortDescription(nameof(MediaSource.Date), ListSortDirection.Descending));

            InitializeComponent();

            ScanMedia();

            MediaList_OnSelectionChanged(null, null);

            CommandManager.InvalidateRequerySuggested();
        }

        private void OpenSettings_OnClick(object sender, RoutedEventArgs e) {
            var settings = new Settings(App.Settings);
            settings.Owner = this;
            settings.ShowDialog();

            App.SaveSettings();
            ScanMedia();

            SelectedMedia = App.Settings.Media.MaxBy(x => x.Date);
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
                    item.UiData.FileStream = new FileStream(item.FileName, FileMode.Open, FileAccess.Read);
                }


                Player.Open(item.UiData.FileStream);
                Player.ShowFrame(0);

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

        #region Waveform dragging

        private bool _isWaveformDragging = false;

        private Point _waveformDragStart = new();

        private void DragWaveform_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _isWaveformDragging = true;
            _waveformDragStart = e.GetPosition(this);
        }

        private void DragWaveform_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            _isWaveformDragging = false;
        }

        private void DragWaveform_OnMouseMove(object sender, MouseEventArgs e) {
            if (_isWaveformDragging) {
                var offset = (long)((_waveformDragStart.X - e.GetPosition(this).X) * DetailWaveformZoom);
                _waveformDragStart = e.GetPosition(this);

                if (offset != 0) {
                    //Player.CurTime += 200;
                    Player.CurTime = Math.Clamp(Player.CurTime + offset, 0, Player.Duration);
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

        #region Selection

        private void SelStart_OnClick(object sender, RoutedEventArgs e) {
            if (SelectedMedia == null) return;

            SelectionStart = Player.CurTime;
            SelectionVisible = true;

            if (SelectionEnd < SelectionStart) {
                SelectionEnd = Player.Duration;
            }

            SelectionLength = SelectionEnd - SelectionStart;
        }

        private void SelEnd_OnClick(object sender, RoutedEventArgs e) {
            if (SelectedMedia == null) return;

            SelectionEnd = Player.CurTime;
            SelectionVisible = true;

            if (SelectionEnd < SelectionStart) {
                SelectionStart = 0;
            }

            SelectionLength = SelectionEnd - SelectionStart;
        }

        private void SelClear_OnClick(object sender, RoutedEventArgs e) {
            SelectionVisible = false;
            SelectionStart = 0;
            SelectionEnd = 0;
        }

        private void SelAdd_OnClick(object sender, RoutedEventArgs e) {
            if (!SelectionVisible) return;

            SelectionVisible = false;

            if (SelectedMedia != null) {
                var json = JsonConvert.SerializeObject(App.Settings.GetDefaultThumbnail());
                var thumb = JsonConvert.DeserializeObject<ThumbnailData>(json);

                var part = new MediaPart {
                    Start = SelectionStart,
                    End = SelectionEnd,
                    Duration = SelectionLength,
                    Color = PartColors[SelectedMedia.Parts.Count % PartColors.Count],
                    Name = $"#{SelectedMedia.Parts.Count}",
                    Thumbnail = thumb,
                    Metadata = new PartMetadata {
                        Date = SelectedMedia.Date
                    },
                    Parent = SelectedMedia
                };

                var dlg = new EditPartMetadata();
                dlg.MediaPart = part;
                dlg.Owner = this;

                if (dlg.ShowDialog() == true) {
                    SelectedMedia.Parts.Add(part);
                    UploadQueue.Add(part);
                    App.SaveSettings();
                }
            }

            SelectionStart = 0;
            SelectionEnd = 0;
        }

        #endregion

        private void PartAreaOverlay_OnPartClicked(MediaPart obj) {
            Player.CurTime = obj.Start;
        }

        private void EditMediaPart_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = e.Parameter is MediaPart;
        }

        private void EditMediaPart_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is MediaPart part) {
                var json = JsonConvert.SerializeObject(part);

                var dlg = new EditPartMetadata();
                dlg.MediaPart = part;
                dlg.Owner = this;

                if (dlg.ShowDialog() != true) {
                    part.Thumbnail = null;
                    part.Metadata = null;
                    JsonConvert.PopulateObject(json, part);
                }
            }
        }

        private void DeleteMediaPart_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is MediaPart part) {
                if (MessageBox.ShowDialog(this, "Soll der Ausschnitt wirklich permanent gelöscht werden?",
                        "Ausschnitt löschen", MessageBoxButton.YesNo) == true) {
                    part.Parent.Parts.Remove(part);
                    UploadQueue.Remove(part);
                }
            }
        }
    }
}