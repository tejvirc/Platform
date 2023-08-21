namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class FontSizeConverter : IValueConverter
    {
        private const double DefaultFontSize = 36;
        private const double DefaultContentHeight = 55;

        /// <summary>
        ///     Use the current content height to determine scaled font size
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && double.TryParse(value.ToString(), out double height) && height > 0)
            {
                return (height / DefaultContentHeight) * DefaultFontSize;
            }

            return DefaultFontSize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
