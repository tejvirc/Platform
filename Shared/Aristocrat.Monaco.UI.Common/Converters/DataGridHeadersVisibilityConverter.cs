namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    ///     Converts a System.Windows.Controls.DataGridHeadersVisibility to a System.Windows.Visibility
    /// </summary>
    public class DataGridHeadersVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///     Convert
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGridHeadersVisibility vis && vis == DataGridHeadersVisibility.None)
            {
                return Visibility.Hidden;
            }

            return DataGrid.HeadersVisibilityConverter.Convert(value, targetType, parameter, culture);
        }

        /// <summary>
        ///     Convert back
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DataGrid.HeadersVisibilityConverter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
