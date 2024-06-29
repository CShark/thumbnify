using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;

namespace tebisCloud.Data.Thumbnail {
    public class TextBoxPart : ControlPart {
        private string _placeholder = "[TextBox]";
        private Color _foreground = Colors.Black;
        private int _fontSize = 20;
        private string _fontFamily = "Arial";
        private double _shadowOpacity = 0;
        private Color _shadowColor = Colors.Black;
        private double _shadowRadius = 10d;
        private double _shadowDirection = 315;
        private double _shadowBlur = 10d;
        private bool _textEditMode;

        public string Placeholder {
            get => _placeholder;
            set => SetField(ref _placeholder, value);
        }

        public Color Foreground {
            get => _foreground;
            set => SetField(ref _foreground, value);
        }

        public int FontSize {
            get => _fontSize;
            set => SetField(ref _fontSize, value);
        }

        public string FontFamily {
            get => _fontFamily;
            set {
                SetField(ref _fontFamily, value);
                OnPropertyChanged(nameof(FontFace));
            }
        }

        public double ShadowOpacity {
            get => _shadowOpacity;
            set => SetField(ref _shadowOpacity, value);
        }

        public Color ShadowColor {
            get => _shadowColor;
            set => SetField(ref _shadowColor, value);
        }

        public double ShadowRadius {
            get => _shadowRadius;
            set => SetField(ref _shadowRadius, value);
        }

        public double ShadowDirection {
            get => _shadowDirection;
            set => SetField(ref _shadowDirection, value);
        }

        public double ShadowBlur {
            get => _shadowBlur;
            set => SetField(ref _shadowBlur, value);
        }

        public override bool FormatingSupport => true;

        [JsonIgnore]
        public FontFamily FontFace {
            get => new(FontFamily);
            set => FontFamily = value.Source;
        }

        [JsonIgnore]
        public bool TextEditMode {
            get => _textEditMode;
            set => SetField(ref _textEditMode, value);
        }
    }
}