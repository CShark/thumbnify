using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace tebisCloud.Converter {

    public class ParameterColor {
        public Type Type { get; set; }
        public Color Color { get; set; }
    }


    [ContentProperty(nameof(Colors))]
    public class ParameterTypeToColor :IValueConverter {
        public List<ParameterColor> Colors { get; set; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Type t) {
                var map = Colors.FirstOrDefault(x => x.Type == t);

                if (map != null) {
                    return map.Color;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
