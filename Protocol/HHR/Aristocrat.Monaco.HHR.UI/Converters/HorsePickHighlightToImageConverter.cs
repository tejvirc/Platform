namespace Aristocrat.Monaco.Hhr.UI.Converters
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public class HorsePickHighlightToImageConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 2)
            {
                return null;
            }

            bool isCorrectPick = (bool)values[0];

            if (!isCorrectPick)
            {
                return null;
            }

            HhrTileImageSize imageSize = (HhrTileImageSize)values[1];

            switch (imageSize)
            {
                case HhrTileImageSize.Large:
                    return (BitmapImage)Util.GetResource(Util.HorseNumberImageLargeHighlightBorderResource);
                case HhrTileImageSize.Medium:
                    return (BitmapImage)Util.GetResource(Util.HorseNumberImageMediumHighlightBorderResource);
                case HhrTileImageSize.Small:
                    return (BitmapImage)Util.GetResource(Util.HorseNumberImageSmallHighlightBorderResource);
            }

            throw new InvalidEnumArgumentException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
