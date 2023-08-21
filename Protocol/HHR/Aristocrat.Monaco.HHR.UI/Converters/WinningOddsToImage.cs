namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// For each odds of winning there is different image needs to place in the Stat's Chart
    /// for example for winning odd 1 its of red color and for 12 its green. This convertor will
    /// will convert the winning odds to corresponding image.
    /// </summary>
    public class WinningOddsToImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is int value1)
            {
                return (BitmapImage)Util.GetResource(Util.WinningOddNumberResource(value1));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}