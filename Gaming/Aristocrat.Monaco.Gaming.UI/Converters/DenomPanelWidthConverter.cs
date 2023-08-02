namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using ViewModels;

    internal class DenomPanelWidthConverter : IMultiValueConverter
    {
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

            // Calculate the width from the number of denoms
            var width = denominations.Count == 0
                ? 0
                : denominations.Count * iconWidth;

            // Account for the margins on each denom
            var marginWidth = denominations.Count * 2 * margin.Left;

            var newWidth = width + marginWidth;

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