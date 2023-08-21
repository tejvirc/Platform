namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using ViewModels;

    public class DenomPanelWidthConverter : IMultiValueConverter
    {
        private readonly double _scaleBy = ScaleUtility.GetScale();

        /// <summary>
        ///     Covert multiple parameters to form the element width
        /// </summary>
        /// <param name="values">the bool values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a value for the width</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var denominations = (ObservableCollection<DenominationInfoViewModel>)values[0]; // Denominations
            var margin = (Thickness)values[1]; // ExtraLargeGameIconDenomIconMargin
            var iconWidth = (double)values[2]; // ExtraLargeGameIconDenomIconWidth
            var scale = (bool)values[3]; // scale by resolution
            var extraPadding = (bool)values[4]; // add more width, done for the viewbox in order to wrap the icons equally on the top and sides

            // Calculate the width from the number of denoms
            var width = denominations.Count == 0
                ? 0
                : denominations.Count * iconWidth;

            // Account for the margins on each denom
            var marginWidth = denominations.Count * 2 * margin.Left;

            // Add additional width if required
            var moreWidth = extraPadding ? 2 * margin.Left : 0;

            // Scale if required
            var newWidth = scale
                ? (width + marginWidth + moreWidth) * _scaleBy
                : width + marginWidth + moreWidth;

            return newWidth;
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