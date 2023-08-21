namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    ///     Converts an array of objects to a delimited concatenated string.
    /// </summary>
    public class DelimitedConverter : IValueConverter
    {
        /// <summary>
        ///     The separator used for the returned delimited string.
        /// </summary>
        public string SeparatorValue { get; set; } = string.Empty;

        /// <summary>
        ///     Creates a concatenated string from the provided values and delimited separator.
        /// </summary>
        /// <param name="value">The items that are passed in.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>A delimited concatenated string.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && TryGetStringArray(value, out string[] items)
                ? string.Join(SeparatorValue, items)
                : string.Empty;
        }

        /// <summary>
        ///     Ignore
        /// </summary>
        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static bool TryGetStringArray(object value, out string[] items)
        {
            items = null;

            if (value is object[] array)
            {
                if (!array.Any())
                {
                    return false;
                }

                var convertedItems = Array.ConvertAll(array, SetObjectToString);
                if (convertedItems.Any())
                {
                    items = convertedItems;

                    return true;
                }
            }

            if (value is IEnumerable enumerable)
            {
                var objects = enumerable.Cast<object>().ToArray();

                if (!objects.Any())
                {
                    return false;
                }

                var convertedItems = Array.ConvertAll(objects, SetObjectToString);
                if (convertedItems.Any())
                {
                    items = convertedItems;
                    return true;
                }
            }

            return false;
        }

        private static string SetObjectToString(object obj) =>
            obj?.ToString() ?? string.Empty;
    }
}
