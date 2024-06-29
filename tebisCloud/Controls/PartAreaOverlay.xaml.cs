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
using tebisCloud.Data;

namespace tebisCloud.Controls {
    /// <summary>
    /// Interaktionslogik für PartAreaOverlay.xaml
    /// </summary>
    public partial class PartAreaOverlay : UserControl {
        public static RoutedUICommand ClickPart = new RoutedUICommand("Select", "selectPart", typeof(PartAreaOverlay));

        public event Action<MediaPart> PartClicked;

        public static readonly DependencyProperty MediaPartsProperty = DependencyProperty.Register(
            nameof(MediaParts), typeof(ObservableCollection<MediaPart>), typeof(PartAreaOverlay),
            new PropertyMetadata(default(ObservableCollection<MediaPart>)));

        public ObservableCollection<MediaPart> MediaParts {
            get { return (ObservableCollection<MediaPart>)GetValue(MediaPartsProperty); }
            set { SetValue(MediaPartsProperty, value); }
        }

        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
            nameof(SelectionStart), typeof(long), typeof(PartAreaOverlay), new PropertyMetadata(default(long)));

        public long SelectionStart {
            get { return (long)GetValue(SelectionStartProperty); }
            set { SetValue(SelectionStartProperty, value); }
        }

        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(
            nameof(SelectionEnd), typeof(long), typeof(PartAreaOverlay), new PropertyMetadata(default(long)));

        public long SelectionEnd {
            get { return (long)GetValue(SelectionEndProperty); }
            set { SetValue(SelectionEndProperty, value); }
        }

        public static readonly DependencyProperty SelectionLengthProperty = DependencyProperty.Register(
            nameof(SelectionLength), typeof(long), typeof(PartAreaOverlay), new PropertyMetadata(default(long)));

        public long SelectionLength {
            get { return (long)GetValue(SelectionLengthProperty); }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public static readonly DependencyProperty SelectionVisibleProperty = DependencyProperty.Register(
            nameof(SelectionVisible), typeof(bool), typeof(PartAreaOverlay), new PropertyMetadata(default(bool)));

        public bool SelectionVisible {
            get { return (bool)GetValue(SelectionVisibleProperty); }
            set { SetValue(SelectionVisibleProperty, value); }
        }

        public static readonly DependencyProperty MediaDurationProperty = DependencyProperty.Register(
            nameof(MediaDuration), typeof(long), typeof(PartAreaOverlay), new PropertyMetadata(default(long)));

        public long MediaDuration {
            get { return (long)GetValue(MediaDurationProperty); }
            set { SetValue(MediaDurationProperty, value); }
        }

        public PartAreaOverlay() {
            InitializeComponent();
        }

        private void ClickPart_OnExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Parameter is MediaPart p) {
                OnPartClicked(p);
            }
        }

        protected virtual void OnPartClicked(MediaPart obj) {
            PartClicked?.Invoke(obj);
        }
    }
}