namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    internal class GameTileMarginConverter : IMultiValueConverter
    {
        private const double LowerMarginShift = -100;

        /// <summary>
        ///     Covert a parameter to form the margin for the game tile
        /// </summary>
        /// <param name="values">the bool values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a value for the visibility setting</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var inputs = (GameGridMarginInputs)values[0]; // Margin inputs
            var isLevelForMultipleGames = (bool)values[1]; // MultipleGameAssociatedSapLevelXEnabled

            if (!inputs.ExtraLargeIconLayout)
            {
                return new Thickness();
            }

            // Handle case where there is a banner above each game tile
            // If the game icon height is so large it pushes the upper major banner and lower denom panel off the grid,
            // pull out the lower margin so that it it may overlap
            if (!isLevelForMultipleGames && inputs.GameIconSize.Height > ScaleUtility.GameIconHeightThreshold)
            {
                // Note: pulling out the lower margin by 100 gives plenty of room for the controls to not be cut off.
                // At this point if the game icon gets any bigger it will not look right anyway
                return new Thickness(0, 0, 0, LowerMarginShift);
            }

            // Handle case where there is a single major banner beneath the grand banner
            if (isLevelForMultipleGames)
            {
                const double shiftUpAmount = -60.0;
                return new Thickness(0, shiftUpAmount, 0, LowerMarginShift);
            }

            return new Thickness();
        }

        /// <summary>
        ///     Convert back
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