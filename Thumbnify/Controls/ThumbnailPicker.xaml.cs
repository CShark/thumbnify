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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Thumbnify.Data;
using Thumbnify.Data.Processing;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Dialogs;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für ThumbnailPicker.xaml
    /// </summary>
    public partial class ThumbnailPicker : UserControl {
        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register(
            nameof(Thumbnail), typeof(ThumbnailParam), typeof(ThumbnailPicker), new PropertyMetadata(default(ThumbnailParam)));

        public ThumbnailParam Thumbnail {
            get { return (ThumbnailParam)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public ThumbnailPicker() {
            InitializeComponent();
        }

        private void OpenThumbnail_OnClick(object sender, RoutedEventArgs e) {
            var thumb = LoadSaveDialog.ShowOpenDialog(Window.GetWindow(this), App.Settings.Thumbnails);
            Thumbnail.ThumbnailPreset = thumb.PresetName;
            Thumbnail.LocalThumbnail = null;
        }

        private void ResetThumbnail_OnClick(object sender, RoutedEventArgs e) {
            Thumbnail.LocalThumbnail = null;
        }

        private void EditThumbnail_OnClick(object sender, RoutedEventArgs e) {
            var editor = new ThumbnailPresetEditor();
            editor.Owner = Window.GetWindow(this);
            editor.Thumbnail = Thumbnail.GetThumbnail();

            var args = new ResolveParamArgs();
            args.RoutedEvent = ThumbnailPreview.ResolveParamsEvent;
            args.Source = this;
            RaiseEvent(args);

            editor.PreviewParameters = args.Parameters;

            editor.ShowDialog();

            Thumbnail.LocalThumbnail = editor.Thumbnail;
        }
    }
}
