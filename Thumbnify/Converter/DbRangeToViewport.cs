using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Thumbnify.Converter {
    class DbRangeToViewport : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length == 2 && values[0] is float minDb && values[1] is float maxDb) {
                var range = maxDb - minDb;

                return new Rect(0, 0, 10 / range, 10 / range);
            } else {
                return new Rect(0, 0, 1, 1);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}