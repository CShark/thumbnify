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
using Thumbnify.Data;

namespace Thumbnify.Dialogs {
    /// <summary>
    /// Interaktionslogik für EditPartMetadata.xaml
    /// </summary>
    public partial class EditPartMetadata : Window {
        public static readonly DependencyProperty PartMetadataProperty = DependencyProperty.Register(
            nameof(PartMetadata), typeof(PartMetadata), typeof(EditPartMetadata),
            new PropertyMetadata(default(PartMetadata)));

        public PartMetadata PartMetadata {
            get { return (PartMetadata)GetValue(PartMetadataProperty); }
            set { SetValue(PartMetadataProperty, value); }
        }

        public EditPartMetadata(PartMetadata metadata) {
            PartMetadata = metadata;
            InitializeComponent();

            txtName.SelectAll();
            txtName.Focus();
        }

        public EditPartMetadata() {
            InitializeComponent();

            txtName.Focus();
        }

        private void OpenPreset_OnClick(object sender, RoutedEventArgs e) {
            ProcessingGraph? result2 = null;

            var result = LoadSaveDialog.ShowOpenDialog(this, App.Settings.Processing, null, () => {
                var dlg = new ProcessingEditor();
                dlg.Owner = this;
                dlg.ShowDialog();

                result2 = dlg.Graph;
                result2.Name = "<Custom>";
            });

            if (result2 != null) result = result2;

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