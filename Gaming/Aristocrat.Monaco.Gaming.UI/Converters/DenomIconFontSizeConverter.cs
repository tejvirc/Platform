namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class DenomIconFontSizeConverter : IMultiValueConverter
    {
        private const double NormalFontSize = 32;
        private const double LargeFontSize = 48;
        private const double ExtraLargeGameIconNormalFontSize = 64;
        private const double NormalScreenHeight = 1080;

        /// <summary>
        ///     Covert multiple parameters to form the font size
        /// </summary>
        /// <param name="values">the bool values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a value for the font size</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var screenHeight = (double)values[0]; // GameControlHeight
            var extraLargeIcons = values[1] is bool && (bool)values[1]; // IsExtraLargeGameIconTabActive

            if(extraLargeIcons)
            {
                return ExtraLargeGameIconNormalFontSize;
            }
            else
            {
                return screenHeight <= NormalScreenHeight 
                    ? NormalFontSize
                    : LargeFontSize;
            }
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
