using Newtonsoft.Json;
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
        public static readonly DependencyProperty PartMetadataProperty = DependencyProperty.Register(
            nameof(PartMetadata), typeof(PartMetadata), typeof(EditPartMetadata), new PropertyMetadata(default(PartMetadata)));

        public PartMetadata PartMetadata {
            get { return (PartMetadata)GetValue(PartMetadataProperty); }
            set { SetValue(PartMetadataProperty, value); }
        }

        public EditPartMetadata() {
            InitializeComponent();
        }

        private void OpenPreset_OnClick(object sender, RoutedEventArgs e) {
            var result = LoadSaveDialog.ShowOpenDialog(this, App.Settings.Processing);

            if (result != null) {
                var json = JsonConvert.SerializeObject(result);
                var copy = JsonConvert.DeserializeObject<ProcessingGraph>(json);

                PartMetadata.ProcessingGraph = copy;
                PartMetadata.UpdateParameters();
            }
        }

        private void EditPreset_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ProcessingEditor();
            dlg.Owner = this;
            dlg.Graph = PartMetadata.ProcessingGraph ?? new();
            
            dlg.ShowDialog();

            PartMetadata.ProcessingGraph = dlg.Graph;
            PartMetadata.ProcessingGraph.Name = "<Custom>";
            PartMetadata.UpdateParameters();
        }

        private void Apply_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}