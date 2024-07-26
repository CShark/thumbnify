using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Thumbnify.Controls {
    [StyleTypedProperty(Property = nameof(Style), StyleTargetType = typeof(ListBox))]
    public class ListBoxNoNav : ListBox {
        protected override void OnKeyDown(KeyEventArgs e) {
            
        }
    }
}