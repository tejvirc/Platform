namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Converts bool to visibility property.  By setting the TrueValue
    ///     and False value properties, the default behavior can be customized.
    ///     In particular, you can use this to invert the default behavior to
    ///     make something collapsed when true.
    ///     Xaml usage:
    ///     converters:TabIndexToVisibilityConverter x:Key="TabIndexConverter"
    ///     Visibility="{Binding GameTabInfo.TabCount, Converter={StaticResource TabIndexConverter}, ConverterParameter=0}"
    /// </summary>
    public class TabIndexToVisibilityConverter : IValueConverter
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
            if (value is int numTabs && parameter is string indexText && !string.IsNullOrEmpty(indexText))
            {
                if (int.TryParse(indexText, out int index))
                {
                    if (index >= 0)
                    {
                        return index < numTabs ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }

            return Visibility.Collapsed;
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
