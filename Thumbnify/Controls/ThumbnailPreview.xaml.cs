using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
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
using ColorPicker.Models;
using Thumbnify.Data;
using Thumbnify.Data.Thumbnail;
using WpfColorFontDialog;
using Path = System.IO.Path;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für ThumbnailPreview.xaml
    /// </summary>
    public partial class ThumbnailPreview : UserControl {
        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register(
            nameof(Thumbnail), typeof(ThumbnailData), typeof(ThumbnailPreview),
            new PropertyMetadata(new ThumbnailData()));

        public ThumbnailData Thumbnail {
            get { return (ThumbnailData)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty SelectedControlProperty = DependencyProperty.Register(
            nameof(SelectedControl), typeof(ControlPart), typeof(ThumbnailPreview),
            new PropertyMetadata(default(ControlPart?), (o, args) => {
                if (args.OldValue is ControlPart ctrlOld) {
                    ctrlOld.IsSelected = false;

                    if (args.OldValue is TextBoxPart txt) {
                        txt.TextEditMode = false;
                    }
                }


                if (args.NewValue is ControlPart ctrlNew) {
                    ctrlNew.IsSelected = true;
                }
            }));

        public static readonly DependencyProperty PreviewMetadataProperty = DependencyProperty.Register(
            nameof(PreviewMetadata), typeof(PartMetadata), typeof(ThumbnailPreview), new PropertyMetadata(default(PartMetadata)));

        public PartMetadata PreviewMetadata {
            get { return (PartMetadata)GetValue(PreviewMetadataProperty); }
            set { SetValue(PreviewMetadataProperty, value); }
        }

        public ControlPart? SelectedControl {
            get { return (ControlPart?)GetValue(SelectedControlProperty); }
            set { SetValue(SelectedControlProperty, value); }
        }

        public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register(
            nameof(EditMode), typeof(bool), typeof(ThumbnailPreview),
            new PropertyMetadata(default(bool), (o, args) => ((ThumbnailPreview)o).SelectedControl = null));

        public bool EditMode {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public ThumbnailPreview() {
            InitializeComponent();
        }

        private bool _ctrlDrag = false;

        private Point _dragStart = new();

        private Point _posStart = new();

        private void Ctrl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement fe && EditMode) {
                if (fe.DataContext is ControlPart ctrl) {
                    SelectedControl = ctrl;
                    _ctrlDrag = true;
                    _dragStart = e.GetPosition(ControlContainer);
                    _posStart = new Point(ctrl.Left, ctrl.Top);
                }

                e.Handled = true;
            }
        }

        private void Ctrl_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            _ctrlDrag = false;
            _dragAspect = null;
        }

        private void Ctrl_OnMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (_ctrlDrag && SelectedControl != null) {
                    var newPos = _posStart + (e.GetPosition(ControlContainer) - _dragStart);

                    SelectedControl.Top = Math.Clamp(newPos.Y, 0, Math.Max(0, 1080 - SelectedControl.Height));
                    SelectedControl.Left = Math.Clamp(newPos.X, 0, Math.Max(0, 1920 - SelectedControl.Width));
                }
            } else {
                _ctrlDrag = false;
                _dragAspect = null;
            }
        }

        private void ThumbnailPreview_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            SelectedControl = null;
            _ctrlDrag = false;
            _dragAspect = null;
        }

        #region Dragging

        private Action<double>? _dragXHandler;

        private Action<double>? _dragYHandler;

        private Action<double>? _aspectXHandler;

        private Action<double>? _aspectYHandler;

        private double? _dragAspect;

        private void ThumbDragStart_OnHandler(object sender, DragStartedEventArgs e) {
            if (sender is FrameworkElement thumb && EditMode) {
                _dragXHandler = null;
                _dragYHandler = null;
                _aspectXHandler = null;
                _aspectYHandler = null;

                if (thumb.HorizontalAlignment == HorizontalAlignment.Left) {
                    _dragXHandler = x => {
                        var left = SelectedControl.Left + x;
                        var right = SelectedControl.Left + SelectedControl.Width;

                        SelectedControl.Left = Math.Clamp(left, 0, SelectedControl.Left + SelectedControl.Width - 20);

                        var width = right - SelectedControl.Left;
                        SelectedControl.Width = Math.Clamp(width, 20, 1920 - SelectedControl.Left);
                    };

                    _aspectXHandler = aspect => {
                        var right = SelectedControl.Left + SelectedControl.Width;
                        var width = SelectedControl.Height / aspect;
                        var left = right - width;

                        SelectedControl.Left = Math.Clamp(left, 0, right - 20);
                        SelectedControl.Width = right - SelectedControl.Left;
                    };
                } else if (thumb.HorizontalAlignment == HorizontalAlignment.Right) {
                    _dragXHandler = x => {
                        SelectedControl.Width += x;

                        SelectedControl.Width = Math.Clamp(SelectedControl.Width, 20, 1920 - SelectedControl.Left);
                    };

                    _aspectXHandler = aspect => {
                        var width = SelectedControl.Height / aspect;
                        SelectedControl.Width = Math.Clamp(width, 20, 1920 - SelectedControl.Left);
                    };
                }

                if (thumb.VerticalAlignment == VerticalAlignment.Top) {
                    _dragYHandler = y => {
                        var top = SelectedControl.Top + y;
                        var bottom = SelectedControl.Top + SelectedControl.Height;

                        SelectedControl.Top = Math.Clamp(top, 0, SelectedControl.Top + SelectedControl.Height - 20);

                        var height = bottom - SelectedControl.Top;
                        SelectedControl.Height = Math.Clamp(height, 20, 1080 - SelectedControl.Top);
                    };

                    _aspectYHandler = aspect => {
                        var bottom = SelectedControl.Top + SelectedControl.Height;
                        var height = SelectedControl.Width * aspect;

                        var top = bottom - height;

                        SelectedControl.Top = Math.Clamp(top, 0, bottom - 20);
                        SelectedControl.Height = bottom - SelectedControl.Top;
                    };
                } else if (thumb.VerticalAlignment == VerticalAlignment.Bottom) {
                    _dragYHandler = y => {
                        SelectedControl.Height += y;

                        SelectedControl.Height = Math.Clamp(SelectedControl.Height, 20, 1080 - SelectedControl.Top);
                    };

                    _aspectYHandler = aspect => {
                        var height = SelectedControl.Width * aspect;
                        SelectedControl.Height = Math.Clamp(height, 20, 1080 - SelectedControl.Top);
                    };
                }
            }
        }

        private void ThumbDragMove_OnHandler(object sender, DragDeltaEventArgs e) {
            if (!EditMode) return;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
                if (_dragAspect == null) {
                    _dragAspect = SelectedControl.Height / SelectedControl.Width;
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