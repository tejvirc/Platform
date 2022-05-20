namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    internal class GameIconJackpotTextVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Covert multiple parameters to form a visibility setting
        /// </summary>
        /// <param name="values">the bool values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a value for the visibility setting</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var hasValue = (bool)values[0]; // HasProgressiveOrBonusValue
            var extraLargeIcons = (bool)values[1]; // IsExtraLargeGameIconTabActive

            return hasValue && !extraLargeIcons ? Visibility.Visible : Visibility.Hidden;
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