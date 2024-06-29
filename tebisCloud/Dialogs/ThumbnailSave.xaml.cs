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
using NAudio.SoundFont;
using tebisCloud.Data;

namespace tebisCloud.Dialogs {
    /// <summary>
    /// Interaktionslogik für ThumbnailSave.xaml
    /// </summary>
    public partial class ThumbnailSave : Window {

        public ObservableCollection<ThumbnailData> Thumbnails { get; set; }

        public static readonly DependencyProperty PresetNameProperty = DependencyProperty.Register(
            nameof(PresetName), typeof(string), typeof(ThumbnailSave), new PropertyMetadata(default(string)));

        public string PresetName {
            get { return (string)GetValue(PresetNameProperty); }
            set { SetValue(PresetNameProperty, value); }
        }

        public ThumbnailSave() {
            Thumbnails = App.Settings.Thumbnails;

            InitializeComponent();
        }

        private void Save_OnClick(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(PresetName)) return;

            PresetName = PresetName.Trim();
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void Thumbnails_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var view = CollectionViewSource.GetDefaultView(Thumbnails);
            
            if (view.CurrentItem is ThumbnailData item) {
                PresetName = item.PresetName;
            }
        }

        private void DeleteThumbnail_OnClick(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement ctrl && ctrl.DataContext is ThumbnailData thumb) {
                if (MessageBox.ShowDialog(this, "Soll das Preset endgültig gelöscht werden?", "Preset Löschen",
                        MessageBoxButton.YesNo) == true) {

                    Thumbnails.Remove(thumb);
                    App.SaveSettings();
                }
            }
        }
    }
}
