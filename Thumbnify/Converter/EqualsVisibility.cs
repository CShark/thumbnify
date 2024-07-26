using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Thumbnify.Converter {
    internal class EqualsVisibility :IValueConverter{
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.Equals(parameter)) {
                return Visibility.Visible;
            } else {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
