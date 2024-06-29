﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tebisCloud.Data.Thumbnail {
    public class ImagePart : ControlPart {
        private string _imageSource;

        public string ImageSource {
            get => _imageSource;
            set => SetField(ref _imageSource, value);
        }

        public override bool FormatingSupport => false;
    }
}