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
    public class ParameterControls {
        public Type Type { get; set; }

        public DataTemplate Template { get; set; }
    }


    [ContentProperty(nameof(ControlMap))]
    public class ParameterTypeToControls :DataTemplateSelector {
        public List<ParameterControls> ControlMap { get; set; } = new();

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var t = item?.GetType();

            var map = ControlMap.FirstOrDefault(x => x.Type == t);

            if (map != null) {
                return map.Template;
            }

            return new DataTemplate();
        }
    }
}
