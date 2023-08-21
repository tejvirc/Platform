namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Converter for simulating visibility in a data table for individual values
    /// </summary>
    public class DataTableValueVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Convert a value, bool pair into a string.  String will be string.Empty if bool is false.
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string output = string.Empty;
            if (values.Length == 2)
            {
                if (values[0] != null && values[1] != null && values[1] is bool visible && visible)
                {
                    output = values[0].ToString();
                }
            }

            return output;
        }

        /// <summary>
        ///     Not used
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }
}