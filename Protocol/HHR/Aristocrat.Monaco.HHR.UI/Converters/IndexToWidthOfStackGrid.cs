namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// This will take the index of ItemControl and will return
    /// the width of background image as width of background image
    /// will decrease while moving the downward direction of levels stack
    /// </summary>

    public class IndexToWidthOfStackGrid : IValueConverter
    {
        private const int MaxWidth = 298;
        private const int WidthDiff = 20;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is int value1)
            {
                var width = MaxWidth - WidthDiff * value1;
                if (width > 0)
                    return width;
            }
            return MaxWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}