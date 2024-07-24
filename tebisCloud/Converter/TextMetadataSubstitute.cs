using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Thumbnify.Data;

namespace Thumbnify.Converter {
    internal class TextMetadataSubstitute : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values.Length == 3) {
                if (values[0] is string s && values[1] is string prediger && values[2] is string thema) {
                    return s.Replace("{Prediger}", prediger)
                        .Replace("{Thema}", thema);
                }
            }

            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}