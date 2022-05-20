namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    ///     Returns True if all values are equal, otherwise returns False
    /// </summary>
    public class EqualityToBoolConverter : IMultiValueConverter
    {
        /// <inheritdoc />
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.Length == 0 || value.Contains(null))
            {
                return false;
            }

            if (value.Length == 1)
            {
                return true;
            }

            return value.All(x => x.Equals(value[0]));
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
