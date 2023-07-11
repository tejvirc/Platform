namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using ViewModels;

    public class GameTabMarginConverter : IValueConverter
    {
        private const int GameTabIndex0 = 0;
        private const int GameTabIndex1 = 1;
        private const int GameTabIndex2 = 2;
        private const int GameTabIndex3 = 3;
        private const int GameTabIndex4 = 4;
        private const int GameTabIndex5 = 5;

        private readonly Thickness _defaultMargin = new Thickness(15, 0, 15, 0);

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
                return _defaultMargin;
            }

            var gameTabInfo = (GameTabInfo)value;

            // The tabs are not sized exactly the same so we need little margin adjustments for each one
            switch (gameTabInfo.TabIndex)
            {
                case GameTabIndex0:
                case GameTabIndex1:
                case GameTabIndex4:
                    return new Thickness(18, 0, 14, 0);

                case GameTabIndex2:
                    return new Thickness(20, 0, 15, 0);

                case GameTabIndex3:
                    return new Thickness(19, 0, 14, 0);

                case GameTabIndex5:
                    return new Thickness(14, 0, 21, 0);

                default:
                    return _defaultMargin;
            }
        }

        /// <summary>
        ///     Convert back
        /// </summary>
        /// <param name="value">the true/false string to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>always throws exception</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
