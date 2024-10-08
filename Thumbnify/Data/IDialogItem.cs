﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Thumbnify.Data {
    public interface IDialogItem {
        public string Name { get; }

        public BitmapSource? Preview { get; }
    }
}