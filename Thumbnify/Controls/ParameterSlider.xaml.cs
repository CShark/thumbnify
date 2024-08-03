using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für ParameterSlider.xaml
    /// </summary>
    public partial class ParameterSlider : UserControl {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(double), typeof(ParameterSlider), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Value {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register(
            nameof(DefaultValue), typeof(double), typeof(ParameterSlider), new PropertyMetadata(default(double)));

        public double DefaultValue {
            get { return (double)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double), typeof(ParameterSlider), new PropertyMetadata(default(double)));

        public double Minimum {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double), typeof(ParameterSlider), new PropertyMetadata(default(double)));

        public double Maximum {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(string), typeof(ParameterSlider), new PropertyMetadata(default(string)));

        public string Header {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty ValueSuffixProperty = DependencyProperty.Register(
            nameof(ValueSuffix), typeof(string), typeof(ParameterSlider), new PropertyMetadata(default(string)));

        public string ValueSuffix {
            get { return (string)GetValue(ValueSuffixProperty); }
            set { SetValue(ValueSuffixProperty, value); }
        }

        public static readonly DependencyProperty TickPlacementProperty = DependencyProperty.Register(
            nameof(TickPlacement), typeof(TickPlacement), typeof(ParameterSlider),
            new PropertyMetadata(default(TickPlacement)));

        public TickPlacement TickPlacement {
            get { return (TickPlacement)GetValue(TickPlacementProperty); }
            set { SetValue(TickPlacementProperty, value); }
        }

        public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(
            nameof(TickFrequency), typeof(double), typeof(ParameterSlider), new PropertyMetadata(default(double)));

        public double TickFrequency {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        public ParameterSlider() {
            InitializeComponent();
        }

        private void Slider_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Value = DefaultValue;
        }
    }
}