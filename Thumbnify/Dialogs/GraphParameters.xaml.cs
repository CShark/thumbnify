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
using Thumbnify.Data;
using Thumbnify.Data.Processing;
using Thumbnify.Tools;

namespace Thumbnify.Dialogs {
    /// <summary>
    /// Interaktionslogik für GraphParameters.xaml
    /// </summary>
    public partial class GraphParameters : Window {
        public static readonly DependencyProperty PartMetadataProperty = DependencyProperty.Register(
            nameof(PartMetadata), typeof(PartMetadata), typeof(GraphParameters), new PropertyMetadata(default(PartMetadata)));

        public PartMetadata PartMetadata {
            get { return (PartMetadata)GetValue(PartMetadataProperty); }
            set { SetValue(PartMetadataProperty, value); }
        }

        public static readonly DependencyProperty MediaFileProperty = DependencyProperty.Register(
            nameof(MediaFile), typeof(string), typeof(GraphParameters), new PropertyMetadata(default(string)));

        public string MediaFile {
            get { return (string)GetValue(MediaFileProperty); }
            set { SetValue(MediaFileProperty, value); }
        }

        public GraphParameters() {
            InitializeComponent();
        }

        private void SelectMediaFile_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new VistaOpenFileDialog();
            dlg.Title = Translate.TranslateControl("MediaFileOpen");
            dlg.Filter = "Video (*.mkv, *.mp4)|*.mkv;*.mp4";

            if (dlg.ShowDialog(this) == true) {
                MediaFile = dlg.FileName;
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void Apply_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void GraphParameters_OnResolveParams(object sender, ResolveParamArgs e) {
            e.Parameters = PartMetadata.Parameters;
        }
    }
}