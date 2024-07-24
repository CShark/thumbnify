using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Thumbnify.Data.Processing.Parameters;
using Vortice.Direct2D1;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für FilePathControl.xaml
    /// </summary>
    public partial class FilePathControl : UserControl {
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            nameof(FilePath), typeof(FilePath), typeof(FilePathControl), new PropertyMetadata(default(FilePath)));

        public FilePath FilePath {
            get { return (FilePath) GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public FilePathControl() {
            InitializeComponent();
        }

        private void SelectPath_OnClick(object sender, RoutedEventArgs e) {
            if (FilePath.Mode == FilePath.EPathMode.Directory) {
                var dlg = new VistaFolderBrowserDialog();
                dlg.SelectedPath = FilePath.FileName;

                if (dlg.ShowDialog(Window.GetWindow(this)) == true) {
                    FilePath.FileName = dlg.SelectedPath;
                }
            } else {
                VistaFileDialog dlg = new VistaOpenFileDialog();

                if (FilePath.Mode == FilePath.EPathMode.SaveFile) {
                    dlg = new VistaSaveFileDialog();
                }

                dlg.AddExtension = true;
                dlg.Title = "Dateipfad auswählen";
                dlg.Filter = FilePath.Filter;
                dlg.FileName = FilePath.FileName;

                if (dlg.ShowDialog(Window.GetWindow(this)) == true) {
                    FilePath.FileName = dlg.FileName;
                }
            }
        }
    }
}
