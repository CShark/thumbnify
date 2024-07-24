using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using tebisCloud.Controls;
using tebisCloud.Data;
using tebisCloud.Data.Thumbnail;
using tebisCloud.Dialogs;
using Path = System.IO.Path;

namespace tebisCloud {
    /// <summary>
    /// Interaktionslogik für ThumbnailPresetEditor.xaml
    /// </summary>
    public partial class ThumbnailPresetEditor : Window {
        public static RoutedUICommand MaximizeControl { get; } = new();
        public static RoutedUICommand OrderFirst { get; } = new();
        public static RoutedUICommand OrderUp { get; } = new();
        public static RoutedUICommand OrderDown { get; } = new();
        public static RoutedUICommand OrderLast { get; } = new();
        public static RoutedUICommand FontSizeInc { get; } = new();
        public static RoutedUICommand FontSizeDec { get; } = new();
        public static RoutedUICommand NewThumbnail { get; } = new();
        public static RoutedUICommand SaveThumbnail { get; } = new();
        public static RoutedUICommand LoadThumbnail { get; } = new();

        public static RoutedUICommand AddImage { get; } = new();
        public static RoutedUICommand AddTextbox { get; } = new();

        public static RoutedUICommand DeleteControl { get; } = new();

        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register(
            nameof(Thumbnail), typeof(ThumbnailData), typeof(ThumbnailPresetEditor),
            new PropertyMetadata(default(ThumbnailData)));

        public ThumbnailData Thumbnail {
            get { return (ThumbnailData)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty SelectedControlProperty = DependencyProperty.Register(
            nameof(SelectedControl), typeof(ControlPart), typeof(ThumbnailPresetEditor),
            new PropertyMetadata(default(ControlPart?)));

        public ControlPart? SelectedControl {
            get { return (ControlPart?)GetValue(SelectedControlProperty); }
            set { SetValue(SelectedControlProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailPreviewProperty = DependencyProperty.Register(
            nameof(ThumbnailPreview), typeof(bool), typeof(ThumbnailPresetEditor), new PropertyMetadata(default(bool)));

        public bool ThumbnailPreview {
            get { return (bool)GetValue(ThumbnailPreviewProperty); }
            set { SetValue(ThumbnailPreviewProperty, value); }
        }

        public static readonly DependencyProperty PreviewMetadataProperty = DependencyProperty.Register(
            nameof(PreviewMetadata), typeof(PartMetadata), typeof(ThumbnailPresetEditor),
            new PropertyMetadata(default(PartMetadata)));

        public PartMetadata PreviewMetadata {
            get { return (PartMetadata)GetValue(PreviewMetadataProperty); }
            set { SetValue(PreviewMetadataProperty, value); }
        }

        public ThumbnailPresetEditor() {
            Thumbnail = new();

            InitializeComponent();

            FontFamilies.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontSizes.ItemsSource = new[] { 10, 12, 16, 18, 20, 24, 28, 32, 48, 64, 72, 94, 128 };
        }


        private void NewThumbnail_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            Thumbnail = new ThumbnailData();
            SelectedControl = null;
        }

        private void AddImage_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dlg.Filter = "Bild-Dateien|*.jpg;*.jpeg;*.png";
            dlg.Title = "Bild öffnen";

            if (dlg.ShowDialog() == true) {
                if (File.Exists(dlg.FileName)) {
                    var ctrl = new ImagePart();

                    using (var stream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read)) {
                        var frame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation,
                            BitmapCacheOption.OnDemand);
                        ctrl.Width = frame.PixelWidth;
                        ctrl.Height = frame.PixelHeight;

                        if (ctrl.Width > 1920) {
                            ctrl.Height *= 1920 / ctrl.Width;
                            ctrl.Width = 1920;
                        }

                        if (ctrl.Height > 1080) {
                            ctrl.Width *= 1080 / ctrl.Height;
                            ctrl.Height = 1080;
                        }
                    }

                    ctrl.ImageSource = dlg.FileName;
                    ctrl.Name = Path.GetFileNameWithoutExtension(dlg.FileName);

                    Thumbnail.Controls.Add(ctrl);
                }
            }
        }

        private void AddTextbox_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            var ctrl = new TextBoxPart();
            ctrl.Width = 200;
            ctrl.Height = 50;

            Thumbnail.Controls.Add(ctrl);
        }

        private void MaximizeControl_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is ControlPart ctrl) {
                ctrl.Top = 0;
                ctrl.Left = 0;
                ctrl.Width = 1920;
                ctrl.Height = 1080;
            }
        }

        private void ControlSelected_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (e.Parameter as ControlPart) != null;
        }

        private void SupportsFormatting_OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (e.Parameter as ControlPart)?.FormatingSupport == true;
        }


        private void OrderFirst_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is ControlPart ctrl) {
                Thumbnail.Controls.Remove(ctrl);
                Thumbnail.Controls.Add(ctrl);
            }
        }

        private void OrderUp_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is ControlPart ctrl) {
                var idx = Thumbnail.Controls.IndexOf(ctrl);
                Thumbnail.Controls.Remove(ctrl);
                idx = Math.Clamp(idx + 1, 0, Thumbnail.Controls.Count);
                Thumbnail.Controls.Insert(idx, ctrl);
            }
        }

        private void OrderDown_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is ControlPart ctrl) {
                var idx = Thumbnail.Controls.IndexOf(ctrl);
                Thumbnail.Controls.Remove(ctrl);
                idx = Math.Clamp(idx - 1, 0, Thumbnail.Controls.Count);
                Thumbnail.Controls.Insert(idx, ctrl);
            }
        }

        private void OrderLast_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is ControlPart ctrl) {
                Thumbnail.Controls.Remove(ctrl);
                Thumbnail.Controls.Insert(0, ctrl);
            }
        }

        private void FontSizeInc_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is TextBoxPart ctrl) {
                ctrl.FontSize += 2;
            }
        }

        private void FontSizeDec_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is TextBoxPart ctrl) {
                ctrl.FontSize -= 2;
                if (ctrl.FontSize < 10) {
                    ctrl.FontSize = 10;
                }
            }
        }

        private void DeleteControl_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is ControlPart ctrl) {
                Thumbnail.Controls.Remove(ctrl);
                SelectedControl = null;
            }
        }

        private void SaveThumbnail_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            var result = LoadSaveDialog.ShowSaveDialog(this, App.Settings.Thumbnails, x => {
                App.Settings.Thumbnails.Remove(x);
                App.SaveSettings();
            });

            if (result != null) {
                var json = JsonConvert.SerializeObject(Thumbnail);
                var copy = JsonConvert.DeserializeObject<ThumbnailData>(json);

                var preview = new ThumbnailPreview();
                preview.Thumbnail = copy;
                preview.Measure(new Size(1920, 1080));
                preview.Arrange(new Rect(0, 0, 1920, 1080));
                preview.UpdateLayout();

                var render = new RenderTargetBitmap(1920, 1080, 96, 96, PixelFormats.Pbgra32);
                render.Render(preview);
                copy.Preview = render;
                copy.Created = DateTime.Now;
                copy.PresetName = result;

                var orig = App.Settings.Thumbnails.FirstOrDefault(x => x.PresetName.ToLower() == result.ToLower());

                if (orig != null) {
                    App.Settings.Thumbnails.Remove(orig);
                }

                App.Settings.Thumbnails.Add(copy);
                App.SaveSettings();
            }
        }

        private void LoadThumbnail_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            var result = LoadSaveDialog.ShowOpenDialog(this, App.Settings.Thumbnails, x => {
                App.Settings.Thumbnails.Remove(x);
                App.SaveSettings();
            });

            if (result != null) {
                var json = JsonConvert.SerializeObject(result);
                var copy = JsonConvert.DeserializeObject<ThumbnailData>(json);

                Thumbnail = copy;
            }
        }
    }
}