namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Given a position number n, return an image of the n'th position container
    /// </summary>
    public class HorsePositionToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return (BitmapImage)Application.Current.Resources[Util.HorsePickResource((int)value)];
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}