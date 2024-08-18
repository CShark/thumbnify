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

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für ShortcutInfo.xaml
    /// </summary>
    public partial class ShortcutInfo : UserControl {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
            nameof(Key), typeof(string), typeof(ShortcutInfo), new PropertyMetadata(default(string?)));

        public string? Key {
            get { return (string?)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(ShortcutInfo), new PropertyMetadata(default(string)));

        public string Description {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty MouseButtonProperty = DependencyProperty.Register(
            nameof(MouseButton), typeof(MouseButton?), typeof(ShortcutInfo),
            new PropertyMetadata(default(MouseButton?)));

        public MouseButton? MouseButton {
            get { return (MouseButton?)GetValue(MouseButtonProperty); }
            set { SetValue(MouseButtonProperty, value); }
        }

        public ShortcutInfo() {
            InitializeComponent();
        }
    }
}