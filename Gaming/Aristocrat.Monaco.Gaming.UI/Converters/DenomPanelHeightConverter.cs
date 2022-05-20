namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Forms;

    internal class DenomPanelHeightConverter : IMultiValueConverter
    {
        private const double BaseScreenHeight = 1080;

        /// <summary>
        ///     Covert multiple parameters to form the element height
        /// </summary>
        /// <param name="values">the bool values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a value for the height</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var margin = (Thickness)values[0]; // ExtraLargeGameIconDenomIconMargin
            var iconHeight = (double)values[1]; // ExtraLargeGameIconDenomIconHeight
            var scale = (bool)values[2]; // scale by resolution

            var height = iconHeight + (margin.Top * 2);

            // Scale if required
            var scaleAmount = Screen.PrimaryScreen.Bounds.Height / BaseScreenHeight;
            var newHeight = scale ? height * scaleAmount : height;
            return newHeight;
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