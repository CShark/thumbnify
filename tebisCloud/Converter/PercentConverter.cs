using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Thumbnify.Converter {
    internal class PercentConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double d) {
                return d * 100;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string s) {
                if (int.TryParse(s.Replace("%", ""), out var p)) {
                    return p / 100d;
                }
            }

            return value;
        }
    }
}