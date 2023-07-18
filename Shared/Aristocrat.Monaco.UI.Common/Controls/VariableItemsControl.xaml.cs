namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;

    /// <summary>
    ///     VariableItemsControl is a collection-bound UserControl that will display a ComboBox when there are
    /// greater than one items in the collection and a TextBlock when there is only one item
    /// </summary>
    public partial class VariableItemsControl : INotifyPropertyChanged
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Aristocrat.Monaco.UI.Common.Controls.VariableItemsControl" /> class.
        /// </summary>
        public VariableItemsControl()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     DependencyProperty for binding ComboBox.ItemsSource
        /// </summary>
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items",
            typeof(ObservableCollection<string>),
            typeof(VariableItemsControl),
            new PropertyMetadata(default(ObservableCollection<string>), Items_PropertyChanged));

        private static void Items_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VariableItemsControl control)
            {
                control.OnPropertyChanged(nameof(Items));
                control.OnPropertyChanged(nameof(ItemsHasMoreThanOneItem));

                if (e.NewValue is ObservableCollection<string> collection)
                {
                    collection.CollectionChanged += (o, args) =>
                        control.OnPropertyChanged(nameof(ItemsHasMoreThanOneItem));
                }
            }
        }

        /// <summary>
        ///     Bind your collection of strings to Items
        /// </summary>
        public ObservableCollection<string> Items
        {
            get => (ObservableCollection<string>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        /// <summary>
        ///     Returns true if the collection contains more than 1 item
        /// </summary>
        public bool ItemsHasMoreThanOneItem => Items != null && Items.Count > 1;

        /// <summary>
        ///     DependencyProperty for binding ComboBox.SelectedItem
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(string),
            typeof(VariableItemsControl),
            new PropertyMetadata(default(string), SelectedItem_PropertyChanged));

        private static void SelectedItem_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VariableItemsControl control)
            {
                control.RaiseAllPropertyChanged();
            }
        }

        /// <summary>
        ///     Bind your selected string to SelectedItem
        /// </summary>
        public string SelectedItem
        {
            get => (string)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        ///     DependencyProperty for binding ComboBox.IsEnabled
        /// </summary>
        public static readonly DependencyProperty ComboBoxIsEnabledProperty = DependencyProperty.Register(
            "ComboBoxIsEnabled",
            typeof(bool),
            typeof(VariableItemsControl),
            new PropertyMetadata(default(bool), ComboBoxIsEnabled_PropertyChanged));

        private static void ComboBoxIsEnabled_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VariableItemsControl control)
            {
                control.RaiseAllPropertyChanged();
            }
        }

        /// <summary>
        ///     Property for binding ComboBox.IsEnabled
        /// </summary>
        public bool ComboBoxIsEnabled
        {
            get => (bool)GetValue(ComboBoxIsEnabledProperty);
            set => SetValue(ComboBoxIsEnabledProperty, value);
        }

        private void RaiseAllPropertyChanged()
        {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(ComboBoxIsEnabled));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
