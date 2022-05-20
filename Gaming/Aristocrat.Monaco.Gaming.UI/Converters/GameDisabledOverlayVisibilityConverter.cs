namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Takes multiple parameters to determine if a grayish tint should cover the 
    ///     game icon when its disabled
    /// </summary>
    internal class GameDisabledOverlayVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Convert from bool to a visibility state
        /// </summary>
        /// <param name="values">the bool values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a visibility state</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var enabled = values[0] is bool && (bool)values[0]; // IsEnabled
            var extraLargeIcons = values[1] is bool && (bool)values[1]; // IsExtraLargeGameIconTabActive

            return extraLargeIcons || enabled ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        ///     Convert a visibility back to bool
        /// </summary>
        /// <param name="value">the true/false string to convert</param>
        /// <param name="targetTypes">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>always throws exception</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}