using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Thumbnify.Data.Processing;
using Thumbnify.Postprocessing;
using WPFLocalizeExtension.Engine;

namespace Thumbnify.Converter {
    internal class NodeNameConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Parameter param) {
                return Translate($"param_{param.Id}", Thread.CurrentThread.CurrentUICulture);
            } else if (value is Result res) {
                if (res.Name != null) {
                    return res.Name;
                } else {
                    return Translate($"param_{res.Id}", Thread.CurrentThread.CurrentUICulture);
                }
            } else if (value is EditorNode node) {
                return Translate(node.TitleId,Thread.CurrentThread.CurrentUICulture);
            }

            return null;
        }

        private string? Translate(string id, CultureInfo culture) {
            return LocalizeDictionary.Instance.GetLocalizedObject("Thumbnify", "Nodes", id, culture) as string;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}