namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System;
    using System.Windows;

    /// <summary>
    ///     FilterByDateButton can be used in place of a DataGrid header to enable filtering of that column by date
    /// </summary>
    [CLSCompliant(false)]
    public partial class FilterByDateButton
    {
        /// <summary>
        ///     FilterByDateButton
        /// </summary>
        public FilterByDateButton()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     HeaderText DependencyProperty
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
            nameof(HeaderText),
            typeof(string),
            typeof(FilterByDateButton),
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
        ///     StartDate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty StartDateProperty = DependencyProperty.Register(
            nameof(StartDate),
            typeof(DateTime?),
            typeof(FilterByDateButton),
            new PropertyMetadata(default(DateTime?)));

        /// <summary>
        ///     The earliest date in the list of items to filter
        /// </summary>
        public DateTime? StartDate
        {
            get => (DateTime?)GetValue(StartDateProperty);
            set => SetValue(StartDateProperty, value);
        }

        /// <summary>
        ///     EndDate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty EndDateProperty = DependencyProperty.Register(
            nameof(EndDate),
            typeof(DateTime?),
            typeof(FilterByDateButton),
            new PropertyMetadata(default(DateTime?)));

        /// <summary>
        ///     The latest date in the list of items to filter
        /// </summary>
        public DateTime? EndDate
        {
            get => (DateTime?)GetValue(EndDateProperty);
            set => SetValue(EndDateProperty, value);
        }

        /// <summary>
        ///     SelectedDate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(FilterByDateButton),
            new PropertyMetadata(default(DateTime?)));

        /// <summary>
        ///     The date chosen to filter items with
        /// </summary>
        public DateTime? SelectedDate
        {
            get => (DateTime?)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }
    }
}
