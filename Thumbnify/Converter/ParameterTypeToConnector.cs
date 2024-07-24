using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Thumbnify.Converter {
    [ContentProperty(nameof(Template))]
    public class ParameterConnector {
        public Type Type { get; set; }
        public ControlTemplate Template { get; set; }
    }

    [ContentProperty(nameof(Connectors))]
    internal class ParameterTypeToConnector :IValueConverter {
        public List<ParameterConnector> Connectors { get; set; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Type t) {
                var map = Connectors.FirstOrDefault(x => x.Type == t);

                if (map != null) {
                    return map.Template;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
