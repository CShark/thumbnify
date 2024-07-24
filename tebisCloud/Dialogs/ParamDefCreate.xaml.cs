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
using Thumbnify.Data.ParamStore;

namespace Thumbnify.Dialogs {
    /// <summary>
    /// Interaktionslogik für ParamDefCreate.xaml
    /// </summary>
    public partial class ParamDefCreate : Window {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
            nameof(Name), typeof(string), typeof(ParamDefCreate), new PropertyMetadata(default(string)));

        public string Name {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty ParamTypeProperty = DependencyProperty.Register(
            nameof(ParamType), typeof(ParamGenerator), typeof(ParamDefCreate),
            new PropertyMetadata(ParamDefinition.SupportedTypes.FirstOrDefault()));

        public ParamGenerator ParamType {
            get { return (ParamGenerator)GetValue(ParamTypeProperty); }
            set { SetValue(ParamTypeProperty, value); }
        }

        public ParamDefCreate() {
            InitializeComponent();
        }

        public static ParamDefinition? ShowDialog(Window owner) {
            var dlg = new ParamDefCreate();
            dlg.Owner = owner;

            if (dlg.ShowDialog() == true) {
                var gen = dlg.ParamType;
                var def = gen.CreateNew();
                def.Name = dlg.Name;
                def.Id = Guid.NewGuid().ToString();
                return def;
            } else {
                return null;
            }
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e) {
            if (ParamType != null && !string.IsNullOrWhiteSpace(Name)) {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}