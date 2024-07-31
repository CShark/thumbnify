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
using Thumbnify.Data.Processing.Parameters;

namespace Thumbnify.Dialogs {
    /// <summary>
    /// Interaktionslogik für CompressorParamEditor.xaml
    /// </summary>
    public partial class CompressorParamEditor : Window {
        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register(
            nameof(Parameters), typeof(CompressorParam), typeof(CompressorParamEditor), new PropertyMetadata(default(CompressorParam)));

        public CompressorParam Parameters {
            get { return (CompressorParam)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }
        
        public CompressorParamEditor() {
            InitializeComponent();

            Parameters = new();
        }
    }
}