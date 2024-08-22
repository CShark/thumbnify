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
using Microsoft.Xaml.Behaviors.Core;
using Ookii.Dialogs.Wpf;
using Thumbnify.Data.ParamStore;
using Thumbnify.Dialogs;
using Thumbnify.Data;
using Vortice.Direct2D1;

namespace Thumbnify {
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings : Window {
        public Config Config { get; set; }

        public static RoutedUICommand RemoveStaticGraph { get; } = new();

        public Settings(Config config) {
            Config = config;
            InitializeComponent();
        }

        public Settings() {
            CommandBindings.Add(new(RemoveStaticGraph, (_, e) => {
                Config.StaticGraphs.Remove(e.Parameter as string);
            }));

            InitializeComponent();
        }

        private void SelectVideoPath_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new VistaFolderBrowserDialog();
            if (dlg.ShowDialog(this) == true) {
                Config.VideoPath = dlg.SelectedPath;
            }
        }

        private void SelectGraph_OnClick(object sender, RoutedEventArgs e) {
            var result = LoadSaveDialog.ShowOpenDialog(this, App.Settings.Processing.Where(x => x.RequiresMediaPart));

            if (result != null) {
                Config.DefaultProcessing = result.Name;
            }
        }

        private void AddGraph_OnClick(object sender, RoutedEventArgs e) {
            var result = LoadSaveDialog.ShowOpenDialog(this, App.Settings.Processing.Where(x => !x.RequiresMediaPart));

            if (result != null) {
                Config.StaticGraphs.Add(result.Name);
            }
        }
    }
}