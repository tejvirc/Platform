namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// </summary>
    public class IndexToVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///  Visibility Converter based off on an index value
        /// </summary>
        /// <param name="value">the bool value to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a visibility state</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter is string indexText && !string.IsNullOrEmpty(indexText))
            {
                if (int.TryParse(indexText, out int index))
                {
                    if (index >= 0)
                    {
                        return index == intValue ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        ///    
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
