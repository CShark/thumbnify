using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tebisCloud.Data;

namespace tebisCloud.Dialogs {
    /// <summary>
    /// Interaktionslogik für LoadSaveDialog.xaml
    /// </summary>
    public partial class LoadSaveDialog : Window {
        public static readonly DependencyProperty DialogItemsProperty = DependencyProperty.Register(
            nameof(DialogItems), typeof(ObservableCollection<IDialogItem>), typeof(LoadSaveDialog),
            new PropertyMetadata(default(ObservableCollection<IDialogItem>)));

        public ObservableCollection<IDialogItem> DialogItems {
            get { return (ObservableCollection<IDialogItem>)GetValue(DialogItemsProperty); }
            set { SetValue(DialogItemsProperty, value); }
        }

        public static readonly DependencyProperty IsSaveDialogProperty = DependencyProperty.Register(
            nameof(IsSaveDialog), typeof(bool), typeof(LoadSaveDialog), new PropertyMetadata(default(bool)));

        public bool IsSaveDialog {
            get { return (bool)GetValue(IsSaveDialogProperty); }
            set { SetValue(IsSaveDialogProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(IDialogItem), typeof(LoadSaveDialog), new PropertyMetadata(
                default(IDialogItem?),
                (o, args) => {
                    if (args.NewValue != null && args.NewValue != args.OldValue) {
                        ((LoadSaveDialog)o).SelectedName = ((IDialogItem)args.NewValue).Name;
                    }
                }));

        public IDialogItem? SelectedItem {
            get { return (IDialogItem?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedNameProperty = DependencyProperty.Register(
            nameof(SelectedName), typeof(string), typeof(LoadSaveDialog), new PropertyMetadata(default(string),
                (o, args) => {
                    if (args.NewValue != null && args.NewValue != args.OldValue) {
                        ((LoadSaveDialog)o).SelectedItem =
                            ((LoadSaveDialog)o).DialogItems.FirstOrDefault(x => x.Name == (string)args.NewValue);
                    }
                }));

        public string SelectedName {
            get { return (string)GetValue(SelectedNameProperty); }
            set { SetValue(SelectedNameProperty, value); }
        }

        public static RoutedUICommand DeleteSelected { get; } = new();

        private Action<IDialogItem>? _deleteHandler;

        public static readonly DependencyProperty ShowDeleteButtonProperty = DependencyProperty.Register(
            nameof(ShowDeleteButton), typeof(bool), typeof(LoadSaveDialog), new PropertyMetadata(default(bool)));

        public bool ShowDeleteButton {
            get { return (bool)GetValue(ShowDeleteButtonProperty); }
            set { SetValue(ShowDeleteButtonProperty, value); }
        }

        public LoadSaveDialog() {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(DeleteSelected, (_, _) => {
                if (_deleteHandler != null) {
                    if (MessageBox.ShowDialog(this, "deletePreset", MessageBoxButton.YesNo) == true) {
                        _deleteHandler(SelectedItem);
                        DialogItems.Remove(SelectedItem);
                        SelectedItem = null;
                        SelectedName = "";
                    }
                }
            }, (_, e) => { e.CanExecute = _deleteHandler != null && SelectedItem != null; }));
        }

        public static T? ShowOpenDialog<T>(Window owner, IEnumerable<T> items, Action<T>? onDelete = null)
            where T : class, IDialogItem {
            var dlg = new LoadSaveDialog();
            dlg.Owner = owner;
            dlg.DialogItems = new ObservableCollection<IDialogItem>(items);
            if (onDelete != null) {
                dlg._deleteHandler = x => onDelete((T)x);
                dlg.ShowDeleteButton = true;
            }

            dlg.IsSaveDialog = false;

            if (dlg.ShowDialog() == true) {
                return dlg.SelectedItem as T;
            } else {
                return null;
            }
        }

        public static string? ShowSaveDialog<T>(Window owner, IEnumerable<T> items, Action<T>? onDelete = null)
            where T : class, IDialogItem {
            var dlg = new LoadSaveDialog();
            dlg.Owner = owner;
            dlg.DialogItems = new ObservableCollection<IDialogItem>(items);
            if (onDelete != null) {
                dlg._deleteHandler = x => onDelete((T)x);
                dlg.ShowDeleteButton = true;
            }

            dlg.IsSaveDialog = true;

            if (dlg.ShowDialog() == true && !string.IsNullOrWhiteSpace(dlg.SelectedName)) {
                return dlg.SelectedName;
            } else {
                return null;
            }
        }

        private void Open_OnClick(object sender, RoutedEventArgs e) {
            if (IsSaveDialog && SelectedItem != null) {
                if (MessageBox.ShowDialog(this, "overwritePreset", MessageBoxButton.YesNo) != true) {
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void ItemList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (SelectedItem != null) {
                Open_OnClick(null, null);
            }
        }
    }
}