namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
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
        ///     *WARNING*
        ///     This converter is only intended to be used with a DataGrid with enough space to expand vertically.
        ///     At least the space should be enough to show all the vertical scroll buttons.
        ///     If the returned height is greater than the container's allowable height, the DataGrid will be clipped.
        ///     You can potentially suffer from a bug that the last row is not displayed(due to clipping).
        ///     To resolve this, make sure enough MinHeight is given to the DataGrid.
        ///     *WARNING*
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

                double horizontalScrollButtonsHeight = 0;
                if (values.Length > 1 && values[1] is FrameworkElement horizontalButtons && horizontalButtons.Visibility != Visibility.Collapsed)
                {
                    if (horizontalButtons.Visibility == Visibility.Visible && horizontalButtons.DesiredSize.Height == 0)
                    {
                        horizontalScrollButtonsHeight = 70;
                    }
                    else
                    {
                        horizontalScrollButtonsHeight = horizontalButtons.DesiredSize.Height;
                    }
                }

                var verticalButtons = values.ElementAtOrDefault(2) as FrameworkElement;
                var verticalScrollButtonsHeight = verticalButtons?.Visibility == Visibility.Visible ? verticalButtons!.DesiredSize.Height : 0;

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

                    // FIX TXM-12510: https://jerry.aristocrat.com/browse/TXM-12510
                    // Store the expanded height which can show all vertical scroll buttons.
                    // This is important, because when all records can fit within this expanded height, the vertical scroll bar will be hidden.
                    // Avoid reducing the height of the data grid later to prevent continuous flickering of the window/dialog caused
                    // by showing/hiding the vertical scroll bar.
                    var minHeightShowingAllVerticalScrollButtons = verticalScrollButtonsHeight + headerHeight;
                    if (verticalButtons?.Visibility == Visibility.Visible
                        && verticalButtons!.Tag == null
                        && verticalScrollButtonsHeight > height)
                    {
                        // Record the minimum height, showing all vertical scroll buttons
                        verticalButtons.Tag = minHeightShowingAllVerticalScrollButtons;
                    }
                    else if (verticalButtons?.Visibility == Visibility.Collapsed
                        && verticalButtons!.Tag is double heightShowingAllVerticalScrollButtons)
                    {
                        // Reducing height will cause flickering of the window/dialog.
                        // Clear Tag so that future resizing of the window/dialog will work.
                        verticalButtons.Tag = null;
                        if (heightShowingAllVerticalScrollButtons >= grid.ActualHeight)
                        {
                            return heightShowingAllVerticalScrollButtons;
                        }
                    }
                    // FIX TXM-12510

                    // If vertical scroll buttons are visible, make sure at least their full height is visible
                    return verticalScrollButtonsHeight > height
                        ? minHeightShowingAllVerticalScrollButtons
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
    }
}
