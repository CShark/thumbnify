using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Thumbnify.Data.ParamStore;

namespace Thumbnify.Data.Processing {
    public delegate void ResolveParamDelegate(object sender, ResolveParamArgs e);

    public class ResolveParamArgs : RoutedEventArgs {
        public ObservableCollection<ParamDefinition>? Parameters { get; set; }
    }
}