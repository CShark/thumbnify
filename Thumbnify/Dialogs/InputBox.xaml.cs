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
using Thumbnify.Tools;

namespace Thumbnify.Dialogs {
    /// <summary>
    /// Interaktionslogik für InputBox.xaml
    /// </summary>
    public partial class InputBox : Window {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(InputBox), new PropertyMetadata(default(string)));

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(InputBox), new PropertyMetadata(default(string)));

        public string Message {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(InputBox), new PropertyMetadata(default(string)));

        public string Value {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public InputBox() {
            InitializeComponent();
            Input.Focus();
        }

        public static string ShowDialog(Window owner, string translationKey, string value = "") {
            var dlg = new InputBox();
            dlg.Owner = owner;
            dlg.Message = Translate.TranslateMessage($"msg_{translationKey}");
            dlg.Title = Translate.TranslateMessage($"title_{translationKey}");
            dlg.Value = value;

            if (dlg.ShowDialog() == true) {
                return dlg.Value;
            } else {
                return value;
            }
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}