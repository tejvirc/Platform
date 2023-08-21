namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class ProgressiveLabelSizeConverter : IValueConverter
    {
        private const double MaxSize = 20;
        private const double MaxSizeLength = 12;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is string text) || text.Length <= MaxSizeLength)
            {
                return MaxSize;
            }

            return Math.Floor(MaxSizeLength / text.Length * MaxSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}