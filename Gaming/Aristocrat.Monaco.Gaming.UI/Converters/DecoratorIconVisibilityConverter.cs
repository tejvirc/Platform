namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Takes multiple parameters to determine if a decorator icon (e.g., new/platinum)
    ///     should be shown.  This is because we collapse an animated decorator icon if the game
    ///     icon is disabled and show a static decorator icon instead.
    /// </summary>
    public class DecoratorIconVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Gets or sets a value indicating whether to invert the value
        /// </summary>
        public bool InvertEnabledValue { get; set; }

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
            var show = values[0] is bool && (bool)values[0]; // IsNew/IsPlatinum
            var isLobbyVisible = values[1] is bool && (bool)values[1]; // IsLobbyVisible
            var gameIconEnabled = values[2] is bool && (bool)values[2]; // IsEnabled
            var extraLargeIcons = values[3] is bool && (bool)values[3]; // IsExtraLargeGameIconTabActive

            if (InvertEnabledValue)
            {
                gameIconEnabled = !gameIconEnabled;
            }

            return show && isLobbyVisible && gameIconEnabled && !extraLargeIcons ? Visibility.Visible : Visibility.Hidden;
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