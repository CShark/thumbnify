using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Thumbnify.Converter {
    class MinConverter :IMultiValueConverter{
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length > 0) {
                return values.Select(x => (double)x).Min();
            } else {
                return 0d;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}