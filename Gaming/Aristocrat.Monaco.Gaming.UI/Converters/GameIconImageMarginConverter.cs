namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class GameIconImageMarginConverter : IValueConverter
    {
        private const double BaseScreenHeight = 1080;
        private const int GameCountsToAdjustMargin = 8;
        private const double TopMarginOffset = -40;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs && !inputs.ExtraLargeIconLayout &&
                inputs.ScreenHeight > BaseScreenHeight)
            {
                // Unfortunately the game icon is not created properly, the icons have some blank spaces above and below
                // the images, so we have to move the image up a bit to display it in correct place.
                if (inputs.GameCount > GameCountsToAdjustMargin)
                {
                    return new Thickness(0, TopMarginOffset, 0, 0);
                }
            }

            return new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
