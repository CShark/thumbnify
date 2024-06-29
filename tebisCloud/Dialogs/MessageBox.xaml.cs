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

namespace tebisCloud.Dialogs {
    /// <summary>
    /// Interaktionslogik für MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(MessageBox), new PropertyMetadata(default(string)));

        public string Title {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(string), typeof(MessageBox), new PropertyMetadata(default(string)));

        public string Message {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty Button1TextProperty = DependencyProperty.Register(
            nameof(Button1Text), typeof(string), typeof(MessageBox), new PropertyMetadata(default(string)));

        public string Button1Text {
            get { return (string)GetValue(Button1TextProperty); }
            set { SetValue(Button1TextProperty, value); }
        }

        public static readonly DependencyProperty Button2TextProperty = DependencyProperty.Register(
            nameof(Button2Text), typeof(string), typeof(MessageBox), new PropertyMetadata(default(string)));

        public string Button2Text {
            get { return (string)GetValue(Button2TextProperty); }
            set { SetValue(Button2TextProperty, value); }
        }

        public static readonly DependencyProperty Button3TextProperty = DependencyProperty.Register(
            nameof(Button3Text), typeof(string), typeof(MessageBox), new PropertyMetadata(default(string)));

        public string Button3Text {
            get { return (string)GetValue(Button3TextProperty); }
            set { SetValue(Button3TextProperty, value); }
        }

        public static readonly DependencyProperty Button2VisibleProperty = DependencyProperty.Register(
            nameof(Button2Visible), typeof(bool), typeof(MessageBox), new PropertyMetadata(default(bool)));

        public bool Button2Visible {
            get { return (bool)GetValue(Button2VisibleProperty); }
            set { SetValue(Button2VisibleProperty, value); }
        }

        public static readonly DependencyProperty Button3VisibleProperty = DependencyProperty.Register(
            nameof(Button3Visible), typeof(bool), typeof(MessageBox), new PropertyMetadata(default(bool)));

        public bool Button3Visible {
            get { return (bool)GetValue(Button3VisibleProperty); }
            set { SetValue(Button3VisibleProperty, value); }
        }

        public ICommand Submit { get; }

        public MessageBox() {
            Submit = new ActionCommand(() => {
                DialogResult = true;
                Close();
            });

            InitializeComponent();
        }

        public static bool? ShowDialog(Window owner, string message, string title, MessageBoxButton buttons) {
            var msgBox = new MessageBox();

            msgBox.Message = message;
            msgBox.Title = title;
            msgBox.Owner = owner;

            switch (buttons) {
                case MessageBoxButton.OK:
                    msgBox.Button1Text = "Ok";
                    msgBox.Button2Visible = false;
                    msgBox.Button3Visible = false;
                    break;
                case MessageBoxButton.YesNo:
                    msgBox.Button1Text = "Ja";
                    msgBox.Button2Text = "Nein";
                    msgBox.Button2Visible = true;
                    msgBox.Button3Visible = false;
                    break;
                case MessageBoxButton.OKCancel:
                    msgBox.Button1Text = "Ok";
                    msgBox.Button2Text = "Abbrechen";
                    msgBox.Button2Visible = true;
                    msgBox.Button3Visible = false;
                    break;
                case MessageBoxButton.YesNoCancel:
                    msgBox.Button1Text = "Ja";
                    msgBox.Button2Text = "Nein";
                    msgBox.Button3Text = "Abbrechen";
                    msgBox.Button2Visible = true;
                    msgBox.Button3Visible = true;
                    break;
            }

            return msgBox.ShowDialog();
            
        }

        private void Button1_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void Button2_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void Button3_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = null;
            Close();
        }
    }
}
