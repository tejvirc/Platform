namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Resources/Tiles folder has the three different sized images for the horse number,This
    /// convertor will take the horse number as argument and will return the path of the small image
    /// of that horse number.
    /// </summary>

    public class HorseNumberToSmallImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is int value1)
            {
                return value1 == 0
                    ? (BitmapImage)Util.GetResource(Util.HorseNumberImageSmallDimmerResource)
                    : (BitmapImage)Util.GetResource(Util.SmallHorseNumberResource(value1));
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}