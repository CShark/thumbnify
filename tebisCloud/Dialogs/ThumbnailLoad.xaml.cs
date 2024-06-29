using System;
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

namespace tebisCloud.Dialogs {
    /// <summary>
    /// Interaktionslogik für ThumbnailLoad.xaml
    /// </summary>
    public partial class ThumbnailLoad : Window {
        
        public ObservableCollection<ThumbnailData> Thumbnails { get; set; }

        public static readonly DependencyProperty SelectedThumbnailProperty = DependencyProperty.Register(
            nameof(SelectedThumbnail), typeof(ThumbnailData), typeof(ThumbnailLoad), new PropertyMetadata(default(ThumbnailData)));

        public ThumbnailData SelectedThumbnail {
            get { return (ThumbnailData)GetValue(SelectedThumbnailProperty); }
            set { SetValue(SelectedThumbnailProperty, value); }
        }

        public ThumbnailLoad() {
            Thumbnails = App.Settings.Thumbnails;

            InitializeComponent();
        }

        private void Load_OnClick(object sender, RoutedEventArgs e) {
            if (SelectedThumbnail != null) {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void DeleteThumbnail_OnClick(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement ctrl && ctrl.DataContext is ThumbnailData thumb) {
                if (MessageBox.ShowDialog(this, "Soll das Preset endgültig gelöscht werden?", "Preset Löschen",
                        MessageBoxButton.YesNo) == true) {

                    Thumbnails.Remove(thumb);
                    SelectedThumbnail = null;
                    App.SaveSettings();
                }
            }
        }
    }
}
