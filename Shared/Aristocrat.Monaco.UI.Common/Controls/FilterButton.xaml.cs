namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using Models;

    /// <summary>
    ///     FilterButton can be used in place of a DataGrid header to enable filtering of that column
    /// </summary>
    [CLSCompliant(false)]
    public partial class FilterButton
    {
        /// <summary>
        ///     FilterButton
        /// </summary>
        public FilterButton()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     HeaderText DependencyProperty
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
            nameof(HeaderText),
            typeof(string),
            typeof(FilterButton),
            new PropertyMetadata(default(string)));

        /// <summary>
        ///     The text to display on the Header filter button
        /// </summary>
        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        /// <summary>
        ///     SelectAllIsChecked DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectAllIsCheckedProperty = DependencyProperty.Register(
            nameof(SelectAllIsChecked),
            typeof(bool?),
            typeof(FilterButton),
            new PropertyMetadata(default(bool?)));

        /// <summary>
        ///     Whether the Select All checkbox is checked
        /// </summary>
        public bool? SelectAllIsChecked
        {
            get => (bool?)GetValue(SelectAllIsCheckedProperty);
            set => SetValue(SelectAllIsCheckedProperty, value);
        }

        /// <summary>
        ///     FilterObjectList DependencyProperty
        /// </summary>
        public static readonly DependencyProperty FilterObjectListProperty = DependencyProperty.Register(
            nameof(FilterObjectList),
            typeof(ObservableCollection<FilterObject>),
            typeof(FilterButton),
            new PropertyMetadata(default(ObservableCollection<FilterObject>)));

        /// <summary>
        ///     The list of FilterObjects to include in the filter popup
        /// </summary>
        public ObservableCollection<FilterObject> FilterObjectList
        {
            get => (ObservableCollection<FilterObject>)GetValue(FilterObjectListProperty);
            set => SetValue(FilterObjectListProperty, value);
        }
    }
}
