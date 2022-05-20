namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class GameIconAlignmentConverter : IValueConverter
    {
        private const HorizontalAlignment DefaultHorizontalAlignment = HorizontalAlignment.Left;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is GameGridMarginInputs inputs && inputs.ExtraLargeIconLayout
                ? HorizontalAlignment.Center
                : DefaultHorizontalAlignment;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
