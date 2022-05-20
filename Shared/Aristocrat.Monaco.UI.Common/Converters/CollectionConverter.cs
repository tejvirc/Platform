namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// IntCollectionToIntConverter
    /// </summary>
    public class CollectionConverter : IValueConverter
    {
        /// <summary>
        ///     Convert from bool to a visibility state
        /// </summary>
        /// <param name="value">the bool value to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a visibility state</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string indexText && !string.IsNullOrEmpty(indexText) && int.TryParse(indexText, out int index) && index >= 0)
            {
                if (value is IEnumerable<int> intCollection && index < intCollection.Count())
                {
                    return intCollection.ElementAt(index);
                }

                if (value is IEnumerable<string> stringCollection && index < stringCollection.Count())
                {
                    return stringCollection.ElementAt(index);
                }
            }

            return null;
        }

        /// <summary>
        ///     Convert a visibility state back to bool
        /// </summary>
        /// <param name="value">the visibility state to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>true, false, or null if value wasn't a visibility state</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}