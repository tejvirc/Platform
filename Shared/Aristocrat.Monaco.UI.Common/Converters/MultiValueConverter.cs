namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Converts an array to a copy of the same array, used to allow MultiBinding in CommandParameter
    /// </summary>
    public class MultiValueConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Convert array of values into a copy of the same array of values
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
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
