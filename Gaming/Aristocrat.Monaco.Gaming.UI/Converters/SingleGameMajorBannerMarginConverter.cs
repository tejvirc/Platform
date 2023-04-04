namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    internal class SingleGameMajorBannerMarginConverter : IMultiValueConverter
    {
        // The percentage of the banner to overlay on top of the game icon.
        private const double BannerOverlayOnGameIconPct = 0.56;
        // If the game icon height passes this amount, we need to shift the banner down a bit
        private const double GameIconHeightThreshold = 406;
        private readonly double _scaleBy = ScaleUtility.GetScale();

        /// <summary>
        ///     Covert a parameter to form the margin for the major banner above the game icon
        /// </summary>
        /// <param name="values">the bool values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a value for the height</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var bannerHeight = (double)values[0]; // Height of LightningLinkMajorBannerImageForSingleGame
            var inputs = (GameGridMarginInputs)values[1]; // MarginInputs

            if (!inputs.ExtraLargeIconLayout)
            {
                return new Thickness();
            }

            // If the game icon height is too great, we must shift up the denom panel by some amount, otherwise
            // it will overlap onto the touch screen to play message.
            var heightDifference = inputs.GameIconSize.Height > GameIconHeightThreshold
                ? inputs.GameIconSize.Height - GameIconHeightThreshold
                : 0;

            var marginTop = -1 * bannerHeight * BannerOverlayOnGameIconPct + heightDifference / 2.0;

            var margin = new Thickness(
                0,
                marginTop * _scaleBy,
                0,
                0);

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
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}