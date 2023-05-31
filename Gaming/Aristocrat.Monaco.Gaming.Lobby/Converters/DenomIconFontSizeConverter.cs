namespace Aristocrat.Monaco.Gaming.Lobby.Converters
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
        ///     Convert multiple parameters to form the font size
        /// </summary>
        /// <param name="values">the values to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a value for the font size</returns>
        public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            // values[0] -> GameControlHeight
            // values[1] -> IsExtraLargeGameIconTabActive
            if (values is not null &&
                values.Length is 2 &&
                values[0] is double screenHeight &&
                values[1] is bool extraLargeIcons)
            {
                if (extraLargeIcons)
                {
                    return ExtraLargeGameIconNormalFontSize;
                }

                return screenHeight <= NormalScreenHeight ? NormalFontSize : LargeFontSize;
            }

            return NormalFontSize;
        }

        /// <summary>
        ///     Convert a visibility back to bool
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
