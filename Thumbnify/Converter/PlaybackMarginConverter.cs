using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Thumbnify.Converter {
    internal class PlaybackMarginConverter :IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length == 2 && values[0] is long l && values[1] is long factor) {
                var offset = new Thickness(-l / (double)factor, 0, 0, 0);
                return offset;
            }

            return new Thickness();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}