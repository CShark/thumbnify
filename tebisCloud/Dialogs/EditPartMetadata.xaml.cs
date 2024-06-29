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
using tebisCloud.Data;

namespace tebisCloud.Dialogs {
    /// <summary>
    /// Interaktionslogik für EditPartMetadata.xaml
    /// </summary>
    public partial class EditPartMetadata : Window {
        public static readonly DependencyProperty MediaPartProperty = DependencyProperty.Register(
            nameof(MediaPart), typeof(MediaPart), typeof(EditPartMetadata), new PropertyMetadata(default(MediaPart)));

        public MediaPart MediaPart {
            get { return (MediaPart)GetValue(MediaPartProperty); }
            set { SetValue(MediaPartProperty, value); }
        }

        public EditPartMetadata() {
            InitializeComponent();
        }

        private void LoadThumbnail_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ThumbnailLoad();
            dlg.Owner = this;

            if (dlg.ShowDialog() == true) {
                MediaPart.Thumbnail = dlg.SelectedThumbnail;
            }
        }

        private void EditThumbnail_OnClick(object sender, RoutedEventArgs e) {
            var editor = new ThumbnailPresetEditor();
            editor.Owner = this;
            editor.Thumbnail = MediaPart.Thumbnail;
            editor.PreviewMetadata = MediaPart.Metadata;
            editor.ShowDialog();
        }

        private void Save_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}