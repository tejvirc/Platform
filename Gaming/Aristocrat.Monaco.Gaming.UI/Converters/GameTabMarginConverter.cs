namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Globalization;

    using Contracts.Models;
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
                return new Thickness();
            }

            var gameTabInfo = (GameTabInfo)value; // Game category

            double left = 0;
            // It looks like only Table Games image is not centered when it appears on tab 3/4/5. It could be 
            // those tab images have slight difference with others. Adding a small margin (left) to make it appearing
            // in the center of tab.
            if (gameTabInfo.Category == GameCategory.Table)
            {
                switch(gameTabInfo.TabIndex)
                {
                    case 2:
                        left = 4;
                        break;
                    case 3:
                    case 4:
                        left = 10;
                        break;
                }
                
            }
            return new Thickness(left, 0, 0, 0);
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
