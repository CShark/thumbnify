using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Thumbnify.Tools {
    internal static class VisualTreeExtensions {
        public static T? GetParentOfType<T>(DependencyObject element) where T : DependencyObject {
            var parent = VisualTreeHelper.GetParent(element);

            if (parent != null) {
                if (parent is T item) {
                    return item;
                } else {
                    return GetParentOfType<T>(parent);
                }
            }

            return null;
        }

        public static bool HasParentOfType<T>(DependencyObject element) where T : DependencyObject {
            return GetParentOfType<T>(element) != null;
        }
    }
}