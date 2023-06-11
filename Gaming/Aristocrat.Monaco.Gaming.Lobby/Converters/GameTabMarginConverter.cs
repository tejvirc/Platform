namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Contracts.Models;
    using Models;

    public class GameTabMarginConverter : IValueConverter
    {
        private const int GameTabIndex2 = 2;
        private const int GameTabIndex3 = 3;
        private const int GameTabIndex4 = 4;
        private const int GameTabIndex2LeftMargin = 4;
        private const int GameTabIndex3And4LeftMargin = 10;

        /// <summary>
        ///     Covert a parameter to form the margin for the game tile
        /// </summary>
        /// <param name="value">the GameTabInfo values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>The margin of game tab image</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Thickness();
            }

            var gameTabInfo = (GameTabInfo)value; // Game category

            double left = 0;
            // It looks like only Table Games image is not centered when it appears on tab 3/4/5. It could be 
            // those tab images have slight difference with others. Adding a small margin (left) to make it appearing
            // in the center of tab.
            if (gameTabInfo.Category == GameCategory.Table)
            {
                switch (gameTabInfo.TabIndex)
                {
                    case GameTabIndex2:
                        left = GameTabIndex2LeftMargin;
                        break;
                    case GameTabIndex3:
                    case GameTabIndex4:
                        left = GameTabIndex3And4LeftMargin;
                        break;
                }

            }
            return new Thickness(left, 0, 0, 0);
        }

        /// <summary>
        ///     Convert back
        /// </summary>
        /// <param name="value">the true/false string to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>always throws exception</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
