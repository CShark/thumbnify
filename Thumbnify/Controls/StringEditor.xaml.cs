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
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Dialogs;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für StringEditor.xaml
    /// </summary>
    public partial class StringEditor : UserControl {
        public static readonly DependencyProperty StringParamProperty = DependencyProperty.Register(
            nameof(StringParam), typeof(StringParam), typeof(StringEditor), new PropertyMetadata(default(StringParam)));

        public StringParam StringParam {
            get { return (StringParam)GetValue(StringParamProperty); }
            set { SetValue(StringParamProperty, value); }
        }

        public StringEditor() {
            InitializeComponent();
        }

        private void EditString_OnClick(object sender, RoutedEventArgs e) {
            StringParam.Value = InputBox.ShowDialog(Window.GetWindow(this), "stringParam", StringParam.Value);
        }
    }
}
