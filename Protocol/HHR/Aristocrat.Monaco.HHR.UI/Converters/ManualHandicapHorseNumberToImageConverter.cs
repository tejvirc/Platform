namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Given a horse number, return a large or small image of the horse number
    /// </summary>
    public class ManualHandicapHorseNumberToImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 2)
            {
                return null;
            }

            int horseNumber = (int)values[0];
            bool horseSelected = (bool)values[1];

            if (horseSelected)
            {
                return horseNumber == 0
                    ? (BitmapImage)Util.GetResource(Util.HorseNumberImageLargeDimmerResource)
                    : (BitmapImage)Util.GetResource(Util.LargeHorseNumberResource(horseNumber));
            }
            else
            {
                return horseNumber == 0
                    ? (BitmapImage)Util.GetResource(Util.HorseNumberImageSmallDimmerResource)
                    : (BitmapImage)Util.GetResource(Util.SmallHorseNumberResource(horseNumber));
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
