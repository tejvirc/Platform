namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Models;

    internal class DenomPanelMarginConverter : IMultiValueConverter
    {
        // Move the denom panel up a bit so that it overlays on 10% of the game icon
        private const double DenomPanelOverlayOnGameIconPct = 1.0 / 10.0;
        // If the game icon passes this height, and there a major multi game sap, shift the denom panel up a bit
        private const double GameIconHeightThresholdMajorSap = 540;

        private readonly double _scaleBy = ScaleUtility.GetScale();

        /// <summary>
        ///     Covert multiple parameters to form the margin for the denomination panel
        /// </summary>
        /// <param name="values">the bool values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a value for the height</returns>
        public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
        {
            var denomPanelHeight = (double)values[0]; // ExtraLargeGameIconDenomIconHeight
            var inputs = (GameGridMarginInputs)values[1]; // MarginInputs
            var isLevelForMultipleGames = (bool)values[2]; // MultipleGameAssociatedSapLevelXEnabled

            if (!inputs.ExtraLargeIconLayout)
            {
                return new Thickness();
            }

            // If the game icon height is too great, we must shift up the denom panel by some amount, otherwise
            // it will overlap onto the touch screen to play message.
            var heightDifference = inputs.GameIconSize.Height > ScaleUtility.GameIconHeightThreshold
                ? inputs.GameIconSize.Height - ScaleUtility.GameIconHeightThreshold
                : 0;

            var heightDifferenceMajorSap = inputs.GameIconSize.Height > GameIconHeightThresholdMajorSap
                ? inputs.GameIconSize.Height - GameIconHeightThresholdMajorSap
                : 0;

            var marginTop = isLevelForMultipleGames
                ? inputs.GameIconSize.Height - inputs.GameIconSize.Height * DenomPanelOverlayOnGameIconPct - heightDifferenceMajorSap / 2.0
                : inputs.GameIconSize.Height - inputs.GameIconSize.Height * DenomPanelOverlayOnGameIconPct - heightDifference / 2.0;

            var margin = new Thickness(
                0,
                marginTop * _scaleBy,
                0,
                -2 * denomPanelHeight * _scaleBy);

            return margin;
        }

        /// <summary>
        ///     Convert back
        /// </summary>
        /// <param name="value">the true/false string to convert</param>
        /// <param name="targetTypes">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>always throws exception</returns>
        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}