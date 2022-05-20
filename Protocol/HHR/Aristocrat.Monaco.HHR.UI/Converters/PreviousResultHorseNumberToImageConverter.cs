namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public class PreviousResultHorseNumberToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            int horseNumber = (int)value;

            return horseNumber == 0
                ? (BitmapImage)Util.GetResource(Util.HorseNumberImageMediumDimmerResource)
                : (BitmapImage)Util.GetResource(Util.MediumHorseNumberResource(horseNumber));
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}