using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class Shortcut {
        public List<string> Keys { get; set; } = new();

        public MouseButton? Mouse { get; set; }

        public Shortcut(MouseButton mouse) {
            Mouse = mouse;
        }

        public Shortcut(params string[] keys) {
            Keys = keys.ToList();
        }

        public Shortcut(MouseButton mouse, params string[] keys) {
            Mouse = mouse;
            Keys = keys.ToList();
        }

        public Shortcut() {
        }
    }

    public class ShortcutData {
        public ObservableCollection<Shortcut> Shortcuts { get; set; } = new();

        public string Description { get; set; }

        public ShortcutData(string description, params Shortcut[] shortcuts) {
            Description = description;
            Shortcuts = new(shortcuts);
        }
    }

    /// <summary>
    /// Interaktionslogik für ShortcutInfo.xaml
    /// </summary>
    public partial class ShortcutInfo : UserControl {
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(ShortcutInfo), new PropertyMetadata(default(string)));

        public string Description {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty ShortcutsProperty = DependencyProperty.Register(
            nameof(Shortcuts), typeof(ObservableCollection<Shortcut>), typeof(ShortcutInfo),
            new PropertyMetadata(default(ObservableCollection<Shortcut>)));

        public ObservableCollection<Shortcut> Shortcuts {
            get { return (ObservableCollection<Shortcut>)GetValue(ShortcutsProperty); }
            set { SetValue(ShortcutsProperty, value); }
        }

        public ShortcutInfo() {
            InitializeComponent();
        }
    }
}