namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    ///     We need to determine in xaml whether a game is enabled.  A game is enabled
    ///     if it satisfies the set filters.
    /// </summary>
    internal class ViewFiltersToBoolConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Convert from filters to bool
        /// </summary>
        /// <param name="values">the filters to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a boolean value</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3)
            {
                return false;
            }

            var denomFilter = values[0] as int? ?? 0;
            var denom = MillicentsToCents(values[1] as long? ?? 0L);
            var gameEnabled = values[2] is bool && (bool)values[2];
            var additionalEnableFlags = values.Skip(3).Cast<bool>().ToArray();

            if (!gameEnabled)
            {
                return false;
            }

            // No filter
            if (denomFilter < 1)
            {
                return true;
            }

            // These are any additional boolean flags that should be checked
            foreach (var check in additionalEnableFlags)
            {
                if (!check)
                {
                    return false;
                }
            }

            return denomFilter == denom;
        }

        /// <summary>
        ///     Convert a filter back to bool
        /// </summary>
        /// <param name="value">the filter to convert</param>
        /// <param name="targetTypes">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>always throws an exception</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private long MillicentsToCents(long value)
        {
            // Assumes no truncation.
            return value / 1000;
        }
    }
}
