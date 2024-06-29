using System;
using System.Collections.Generic;
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
using Ookii.Dialogs.Wpf;
using tebisCloud.Data;
using tebisCloud.Dialogs;

namespace tebisCloud {
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings : Window {
        public Config Config { get; set; }

        public static readonly DependencyProperty DefaultThumbnailProperty = DependencyProperty.Register(
            nameof(DefaultThumbnail), typeof(ThumbnailData), typeof(Settings), new PropertyMetadata(default(ThumbnailData)));

        public ThumbnailData? DefaultThumbnail {
            get { return (ThumbnailData?)GetValue(DefaultThumbnailProperty); }
            set { SetValue(DefaultThumbnailProperty, value); }
        }

        public Settings(Config config) {
            Config = config;
            DefaultThumbnail = config.GetDefaultThumbnail();
            InitializeComponent();
        }

        public Settings() {
            InitializeComponent();
        }

        private void SelectVideoPath_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new VistaFolderBrowserDialog();
            if (dlg.ShowDialog(this) == true) {
                Config.VideoPath = dlg.SelectedPath;
            }
        }

        private void EditThumbPresets_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ThumbnailPresetEditor();
            dlg.Owner = this;
            dlg.ShowDialog();
        }

        private void DefaultThumbnail_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var dlg = new ThumbnailLoad();
            dlg.Owner = this;
            dlg.SelectedThumbnail = DefaultThumbnail;

            if (dlg.ShowDialog() == true) {
                DefaultThumbnail = dlg.SelectedThumbnail;
                Config.DefaultThumbnail = DefaultThumbnail.PresetName;
            }
        }
    }
}