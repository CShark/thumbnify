using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thumbnify.Data.Thumbnail;
using Vortice.Direct2D1;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für ControlManipulator.xaml
    /// </summary>
    public partial class ControlManipulator : UserControl {
        public static readonly DependencyProperty TargetControlProperty = DependencyProperty.Register(
            nameof(TargetControl), typeof(ControlPart), typeof(ControlManipulator),
            new PropertyMetadata(default(ControlPart)));

        public ControlPart TargetControl {
            get { return (ControlPart)GetValue(TargetControlProperty); }
            set { SetValue(TargetControlProperty, value); }
        }

        public ControlManipulator() {
            InitializeComponent();
        }

        #region Dragging

        private Action<double>? _dragXHandler;

        private Action<double>? _dragYHandler;

        private Action<double>? _aspectXHandler;

        private Action<double>? _aspectYHandler;

        private double? _dragAspect;

        private void ThumbDragStart_OnHandler(object sender, DragStartedEventArgs e) {
            if (sender is FrameworkElement thumb && IsEnabled) {
                _dragXHandler = null;
                _dragYHandler = null;
                _aspectXHandler = null;
                _aspectYHandler = null;

                if (thumb.HorizontalAlignment == HorizontalAlignment.Left) {
                    _dragXHandler = x => {
                        var left = TargetControl.Left + x;
                        var right = TargetControl.Left + TargetControl.Width;

                        TargetControl.Left = Math.Clamp(left, 0, TargetControl.Left + TargetControl.Width - 20);

                        var width = right - TargetControl.Left;
                        TargetControl.Width = Math.Clamp(width, 20, 1920 - TargetControl.Left);
                    };

                    _aspectXHandler = aspect => {
                        var right = TargetControl.Left + TargetControl.Width;
                        var width = TargetControl.Height / aspect;
                        var left = right - width;

                        TargetControl.Left = Math.Clamp(left, 0, right - 20);
                        TargetControl.Width = right - TargetControl.Left;
                    };
                } else if (thumb.HorizontalAlignment == HorizontalAlignment.Right) {
                    _dragXHandler = x => {
                        TargetControl.Width += x;

                        TargetControl.Width = Math.Clamp(TargetControl.Width, 20, 1920 - TargetControl.Left);
                    };

                    _aspectXHandler = aspect => {
                        var width = TargetControl.Height / aspect;
                        TargetControl.Width = Math.Clamp(width, 20, 1920 - TargetControl.Left);
                    };
                }

                if (thumb.VerticalAlignment == VerticalAlignment.Top) {
                    _dragYHandler = y => {
                        var top = TargetControl.Top + y;
                        var bottom = TargetControl.Top + TargetControl.Height;

                        TargetControl.Top = Math.Clamp(top, 0, TargetControl.Top + TargetControl.Height - 20);

                        var height = bottom - TargetControl.Top;
                        TargetControl.Height = Math.Clamp(height, 20, 1080 - TargetControl.Top);
                    };

                    _aspectYHandler = aspect => {
                        var bottom = TargetControl.Top + TargetControl.Height;
                        var height = TargetControl.Width * aspect;

                        var top = bottom - height;

                        TargetControl.Top = Math.Clamp(top, 0, bottom - 20);
                        TargetControl.Height = bottom - TargetControl.Top;
                    };
                } else if (thumb.VerticalAlignment == VerticalAlignment.Bottom) {
                    _dragYHandler = y => {
                        TargetControl.Height += y;

                        TargetControl.Height = Math.Clamp(TargetControl.Height, 20, 1080 - TargetControl.Top);
                    };

                    _aspectYHandler = aspect => {
                        var height = TargetControl.Width * aspect;
                        TargetControl.Height = Math.Clamp(height, 20, 1080 - TargetControl.Top);
                    };
                }
            }
        }

        private void ThumbDragMove_OnHandler(object sender, DragDeltaEventArgs e) {
            if (!IsEnabled) return;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
                if (_dragAspect == null) {
                    _dragAspect = TargetControl.Height / TargetControl.Width;
                }

                if (_dragXHandler != null) {
                    _dragXHandler?.Invoke(e.HorizontalChange);
                    _aspectYHandler?.Invoke(_dragAspect.Value);
                    _aspectXHandler?.Invoke(_dragAspect.Value);
                } else {
                    _dragYHandler?.Invoke(e.VerticalChange);
                    _aspectXHandler?.Invoke(_dragAspect.Value);
                    _aspectYHandler?.Invoke(_dragAspect.Value);
                }
            } else {
                _dragXHandler?.Invoke(e.HorizontalChange);
                _dragYHandler?.Invoke(e.VerticalChange);

                _dragAspect = null;
            }
        }

        private void ThumbDragEnd_OnHandler(object sender, DragCompletedEventArgs e) {
            _dragXHandler = null;
            _dragYHandler = null;
            _aspectXHandler = null;
            _aspectYHandler = null;
            _dragAspect = null;
        }

        #endregion
    }
}