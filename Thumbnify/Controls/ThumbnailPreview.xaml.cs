﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.RegularExpressions;
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
using Thumbnify.Data.ParamStore;
using Thumbnify.Data.Processing;
using Thumbnify.Data.Processing.Operations;
using Thumbnify.Data.Processing.Parameters;
using Thumbnify.Data.Thumbnail;
using WpfColorFontDialog;
using Path = System.IO.Path;

namespace Thumbnify.Controls {
    /// <summary>
    /// Interaktionslogik für ThumbnailPreview.xaml
    /// </summary>
    public partial class ThumbnailPreview : UserControl {
        private Timer _previewTimer;

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
                }


                if (args.NewValue is ControlPart ctrlNew) {
                    ctrlNew.IsSelected = true;
                }
            }));

        public static readonly DependencyProperty PreviewMetadataProperty = DependencyProperty.Register(
            nameof(PreviewMetadata), typeof(PartMetadata), typeof(ThumbnailPreview),
            new PropertyMetadata(default(PartMetadata)));

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

        public static RoutedEvent ResolveParamsEvent = EventManager.RegisterRoutedEvent(nameof(ResolveParams),
            RoutingStrategy.Bubble, typeof(ResolveParamDelegate), typeof(ThumbnailPreview));

        public event ResolveParamDelegate ResolveParams {
            add => AddHandler(ResolveParamsEvent, value);
            remove => RemoveHandler(ResolveParamsEvent, value);
        }

        public static readonly DependencyProperty LivePreviewProperty = DependencyProperty.Register(
            nameof(LivePreview), typeof(bool), typeof(ThumbnailPreview),
            new PropertyMetadata(default(bool), (o, _) => ((ThumbnailPreview)o).UpdatePreviewStatus()));

        public bool LivePreview {
            get { return (bool)GetValue(LivePreviewProperty); }
            set { SetValue(LivePreviewProperty, value); }
        }

        public ThumbnailPreview() {
            _previewTimer = new(UpdatePreview);
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
            }
        }

        private void ThumbnailPreview_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            SelectedControl = null;
            _ctrlDrag = false;
        }

        private void UpdatePreviewStatus() {
            if (!DesignerProperties.GetIsInDesignMode(this)) {
                if (LivePreview) {
                    _previewTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(250));
                } else {
                    _previewTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                }
            }
        }
        

        private void UpdatePreview(object? state) {
            var args = new ResolveParamArgs();
            args.RoutedEvent = ResolveParamsEvent;
            args.Source = this;

            try {
                Dispatcher.Invoke(() => {
                    RaiseEvent(args);

                    if (Thumbnail != null) {
                        SetParameters(args.Parameters);
                       
                    }
                });
            } catch (TaskCanceledException ex) {
            }
        }

        public void SetParameters(ObservableCollection<ParamDefinition>? paramList) {
            foreach (var part in Thumbnail.Controls.OfType<TextBoxPart>()) {
                if (paramList == null) {
                    part.PreviewText = null;
                } else {
                    part.PreviewText = TextReplace.ReplaceVariables(part.Placeholder, paramList);
                }
            }
        }
    }
}