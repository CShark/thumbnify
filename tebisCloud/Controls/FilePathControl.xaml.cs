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
using tebisCloud.Data.Processing.Parameters;

namespace tebisCloud.Controls {
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
            VistaFileDialog dlg = new VistaSaveFileDialog();
            
            if (FilePath.FileMustExist) {
                dlg = new VistaOpenFileDialog();
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
