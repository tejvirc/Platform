namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    ///     Evaluate the Visibility of the vertical scroll bar in a DataGrid.
    /// </summary>
    public class DataGridVerticalScrollBarVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        ///   Return Visibility property of the vertical scroll bar in a DataGrid.
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 1 && values[0] is DataGrid grid)
            {
                var scrollViewer = grid.Template.FindName("DG_ScrollViewer", grid) as ScrollViewer;
                if (scrollViewer?.VerticalOffset > 0)
                {
                    return Visibility.Visible;
                }

                var row = grid.FindChild<DataGridRow>();
                if (row != null && !double.IsNaN(row.ActualHeight) && row.ActualHeight > 0)
                {
                    var horizontalScrollButtonsHeight = DataGridHeightConverter.GetHorizontalScrollButtonsHeight(grid);
                    return grid.ActualHeight < GetItemsSourceCount(grid) * row.ActualHeight + grid.ColumnHeaderHeight + horizontalScrollButtonsHeight ? Visibility.Visible : Visibility.Collapsed;
                }

                return values[1] is double scrollableHeight && scrollableHeight > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        ///     Not used
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private static int GetItemsSourceCount(DataGrid dataGrid)
        {
            var itemsSource = dataGrid.ItemsSource;
            var countProperty = itemsSource?.GetType().GetProperty("Count");
            var itemsCount = (int?)countProperty?.GetValue(itemsSource);
            return itemsCount ?? dataGrid.Items.Count;
        }
    }
}