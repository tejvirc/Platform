namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using ViewModels;

    public class GameTabMarginConverter : IValueConverter
    {
        /// <summary>
        ///     Covert a parameter to form the margin for the game tile
        /// </summary>
        /// <param name="value">the GameTabInfo values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>The margin of game tab image</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Thickness(15, 0, 15, 0);
            }

            var gameTabInfo = (GameTabInfo)value;

            // The tabs are not sized exactly the same so we need little margin adjustments for each one
            switch (gameTabInfo.TabIndex)
            {
                case 0:
                case 1:
                case 4:
                    return new Thickness(18, 0, 14, 0);
                case 2:
                    return new Thickness(20, 0, 15, 0);
                case 3:
                    return new Thickness(19, 0, 14, 0);
                case 5:
                    return new Thickness(14, 0, 21, 0);
                default:
                    return new Thickness(15, 0, 15, 0);
            }
        }

        /// <summary>
        ///     Convert back
        /// </summary>
        /// <param name="value">the true/false string to convert</param>
        /// <param name="targetTypes">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>always throws exception</returns>
        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
