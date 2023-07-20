namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;

    /// <summary>
    ///     Calculate the height of a DataGrid to display full rows only, not partial rows
    /// </summary>
    public class DataGridHeightConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Convert DataGrid to calculated height
        /// </summary>
        /// <param name="values">
        /// values[0] should be a DataGrid
        /// values[1] should be the horizontal scroll buttons control
        /// values[2] should be the vertical scroll buttons control
        /// values[3] should be the ScrollableHeight of the ScrollViewer, necessary to recalculate when the grid height has changed
        /// values[4] should be the ScrollableWidth of the ScrollViewer, necessary to recalculate when the grid width has changed
        /// </param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">not used</param>
        /// <param name="culture">not used</param>
        /// <returns>A double representing the calculated grid height</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 1 && values[0] is DataGrid grid)
            {
                var actualHeight = grid.ActualHeight;

                var horizontalScrollButtonsHeight = GetHorizontalScrollButtonsHeight(grid);
                var verticalScrollButtonsHeight = values.Length > 2 && values[2] is FrameworkElement verticalButtons && verticalButtons.Visibility == Visibility.Visible
                    ? verticalButtons.DesiredSize.Height
                    : 0;

                var header = grid.FindChild<DataGridColumnHeadersPresenter>();
                var headerHeight = header?.ActualHeight ?? 0;

                var row = grid.FindChild<DataGridRow>();
                if (row != null && !double.IsNaN(row.ActualHeight) && row.ActualHeight > 0)
                {
                    var rowsHeight = actualHeight - grid.ColumnHeaderHeight - horizontalScrollButtonsHeight;
                    // Show a minimum of 2 rows
                    // Otherwise show the maximum number of FULL height rows possible, no partial height rows
                    var height = Math.Max(
                        Math.Floor(rowsHeight / row.ActualHeight) * row.ActualHeight,
                        row.ActualHeight * 2);

                    // If vertical scroll buttons are visible, make sure at least their full height is visible
                    return verticalScrollButtonsHeight > height
                        ? verticalScrollButtonsHeight + headerHeight
                        : height + headerHeight;
                }

                // If the ActualHeight and header height are similar, we should return Auto height because
                // it means the DataGrid.ItemsSource is empty or hasn't been set yet
                if (Math.Abs(actualHeight - headerHeight) > 10)
                {
                    return actualHeight;
                }
            }

            // Return Auto height
            return double.NaN;
        }

        /// <summary>
        ///     Not used
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        internal static double GetHorizontalScrollButtonsHeight(DataGrid grid)
        {
            double result = 0;
            if (grid.Template.FindName("PART_GridHorizontalScrollButtons", grid) is FrameworkElement horizontalButtons && horizontalButtons.Visibility != Visibility.Collapsed)
            {
                if (horizontalButtons.Visibility == Visibility.Visible && horizontalButtons.DesiredSize.Height == 0)
                {
                    result = 70;
                }
                else
                {
                    result = horizontalButtons.DesiredSize.Height;
                }
            }

            return result;
        }
    }
}
