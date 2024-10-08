﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thumbnify.Data;
using Vortice.MediaFoundation;

namespace Thumbnify.Controls {
    public enum PartSelectionMode {
        None,
        Start,
        End,
        All,
    }

    /// <summary>
    /// Interaktionslogik für PartAreaOverlay.xaml
    /// </summary>
    public partial class PartAreaOverlay : UserControl {
        public static RoutedUICommand ClickPart = new();
        public static RoutedUICommand RightClickPart = new();

        public event Action<MediaPart> PartClicked;

        public static readonly DependencyProperty MediaPartsProperty = DependencyProperty.Register(
            nameof(MediaParts), typeof(ObservableCollection<MediaPart>), typeof(PartAreaOverlay),
            new PropertyMetadata(default(ObservableCollection<MediaPart>),
                (o, args) => ((PartAreaOverlay)o).SelectedMediaPart = null));

        public ObservableCollection<MediaPart> MediaParts {
            get { return (ObservableCollection<MediaPart>)GetValue(MediaPartsProperty); }
            set { SetValue(MediaPartsProperty, value); }
        }

        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
            nameof(SelectionStart), typeof(long), typeof(PartAreaOverlay), new PropertyMetadata(default(long)));

        public long SelectionStart {
            get { return (long)GetValue(SelectionStartProperty); }
            set { SetValue(SelectionStartProperty, value); }
        }

        public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(
            nameof(SelectionEnd), typeof(long), typeof(PartAreaOverlay), new PropertyMetadata(default(long)));

        public long SelectionEnd {
            get { return (long)GetValue(SelectionEndProperty); }
            set { SetValue(SelectionEndProperty, value); }
        }

        public static readonly DependencyProperty SelectionLengthProperty = DependencyProperty.Register(
            nameof(SelectionLength), typeof(long), typeof(PartAreaOverlay), new PropertyMetadata(default(long)));

        public long SelectionLength {
            get { return (long)GetValue(SelectionLengthProperty); }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public static readonly DependencyProperty SelectionVisibleProperty = DependencyProperty.Register(
            nameof(SelectionVisible), typeof(bool), typeof(PartAreaOverlay), new PropertyMetadata(default(bool)));

        public bool SelectionVisible {
            get { return (bool)GetValue(SelectionVisibleProperty); }
            set { SetValue(SelectionVisibleProperty, value); }
        }

        public static readonly DependencyProperty MediaDurationProperty = DependencyProperty.Register(
            nameof(MediaDuration), typeof(long), typeof(PartAreaOverlay), new PropertyMetadata(default(long)));

        public long MediaDuration {
            get { return (long)GetValue(MediaDurationProperty); }
            set { SetValue(MediaDurationProperty, value); }
        }

        public static readonly DependencyProperty OverlayOpacityProperty = DependencyProperty.Register(
            nameof(OverlayOpacity), typeof(double), typeof(PartAreaOverlay), new PropertyMetadata(1.0d));

        public double OverlayOpacity {
            get { return (double)GetValue(OverlayOpacityProperty); }
            set { SetValue(OverlayOpacityProperty, value); }
        }

        public static readonly DependencyProperty ShowLabelsProperty = DependencyProperty.Register(
            nameof(ShowLabels), typeof(bool), typeof(PartAreaOverlay), new PropertyMetadata(default(bool)));

        public bool ShowLabels {
            get { return (bool)GetValue(ShowLabelsProperty); }
            set { SetValue(ShowLabelsProperty, value); }
        }

        public static readonly DependencyProperty SelectedMediaPartProperty = DependencyProperty.Register(
            nameof(SelectedMediaPart), typeof(MediaPart), typeof(PartAreaOverlay),
            new FrameworkPropertyMetadata(default(MediaPart?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public MediaPart? SelectedMediaPart {
            get { return (MediaPart?)GetValue(SelectedMediaPartProperty); }
            set { SetValue(SelectedMediaPartProperty, value); }
        }
        
        public static readonly DependencyProperty PartSelectionModeProperty = DependencyProperty.Register(
            nameof(PartSelectionMode), typeof(PartSelectionMode), typeof(PartAreaOverlay), new PropertyMetadata(PartSelectionMode.None));

        public PartSelectionMode PartSelectionMode {
            get { return (PartSelectionMode)GetValue(PartSelectionModeProperty); }
            set { SetValue(PartSelectionModeProperty, value); }
        }

        public PartAreaOverlay() {
            InitializeComponent();

            CommandBindings.Add(new(ClickPart, (_, e) => {
                if (e.Parameter is MediaPart p) {
                    OnPartClicked(p);
                    ShowPopup(e.OriginalSource as UIElement, p);
                }
            }));

            CommandBindings.Add(new(RightClickPart, (_, e) => {
                if (e.Parameter is MediaPart p) {
                    ShowPopup(e.OriginalSource as UIElement, p);
                }
            }));
        }

        private void ShowPopup(UIElement? target, MediaPart part) {
            if (target == null) return;

            PopupControls.PlacementTarget = target;
            PopupControls.IsOpen = true;
            PopupControls.StaysOpen = true;
            PopupControls.DataContext = part;
            CommandManager.InvalidateRequerySuggested();

            Window.GetWindow(this).PreviewMouseLeftButtonUp += Window_OnMouseLeftButtonUp;
            Window.GetWindow(this).MouseLeave += Window_OnMouseLeave;
        }

        private void Window_OnMouseLeave(object sender, MouseEventArgs e) {
            Window_OnMouseLeftButtonUp(null, null);
        }

        private void Window_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            PopupControls.StaysOpen = false;

            Window.GetWindow(this).PreviewMouseLeftButtonUp -= Window_OnMouseLeftButtonUp;
            Window.GetWindow(this).MouseLeave -= Window_OnMouseLeave;
        }


        protected virtual void OnPartClicked(MediaPart obj) {
            SelectedMediaPart = obj;
            PartClicked?.Invoke(obj);
        }

        private void PopupControls_OnClosed(object? sender, EventArgs e) {
            Window.GetWindow(this).PreviewMouseLeftButtonUp -= Window_OnMouseLeftButtonUp;
            Window.GetWindow(this).MouseLeave -= Window_OnMouseLeave;
        }

        private void PopupControls_OnPreviewMouseUp(object sender, MouseButtonEventArgs e) {
            PopupControls.IsOpen = false;
        }
    }
}