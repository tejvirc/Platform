namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Forms;

    public class GameIconHeightConverter : IValueConverter
    {
        private const double SmallIconHeight = 259;
        private const double LargeIconHeight = 308;
        private const int GameCountSize = 8;
        private const double BaseScreenHeight = 1080;
        private const int GameHeightWithSubTabAdjustment = 30;
        private readonly double _scaleBy = Screen.PrimaryScreen.Bounds.Height / BaseScreenHeight;


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return LargeIconHeight;
            }

            if (value is GameGridMarginInputs inputs)
            {
                double result;
                if (inputs.ExtraLargeIconLayout)
                {
                    result = inputs.ScreenHeight > BaseScreenHeight
                        ? inputs.GameIconSize.Height * _scaleBy
                        : inputs.GameIconSize.Height;
                }
                else
                {
                    // Lobby layout icon sizes based on number of games. Affects the size of the icon image
                    var size = inputs.GameCount > GameCountSize || inputs.TabView ? SmallIconHeight : LargeIconHeight;
                    var scaleBy = inputs.ScreenHeight / BaseScreenHeight;
                    result = size * scaleBy;

                    if (inputs.GameCount > GameCountSize && inputs.SubTabVisible)
                    {
                        // If there are sub tabs, it means we will have less space on the screen
                        // so we have to reduce the image height a bit
                        result -= GameHeightWithSubTabAdjustment;
                    }
                }

                return result;
            }

            return LargeIconHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
