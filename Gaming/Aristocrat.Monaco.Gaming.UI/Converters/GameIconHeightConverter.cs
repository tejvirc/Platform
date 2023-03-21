namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Data;
    using log4net;

    public class GameIconHeightConverter : IValueConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Who came up with these magic numbers? Why were they chosen?
        private const double SmallIconHeight = 259;
        private const double LargeIconHeight = 308;
        private const int GameCountSize = 8;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs)
            {
                double result;
                var scaleBy = inputs.GameWindowHeight / ScaleUtility.BaseScreenHeight;
                if (inputs.ExtraLargeIconLayout)
                {
                    result = inputs.GameWindowHeight > ScaleUtility.BaseScreenHeight
                        ? inputs.GameIconSize.Height * scaleBy
                        : inputs.GameIconSize.Height;
                }
                else
                {
                    // Lobby layout icon sizes based on number of games. Affects the size of the icon image
                    var size = inputs.GameCount > GameCountSize || inputs.TabView ? SmallIconHeight : LargeIconHeight;
                    result = size * scaleBy;

                    if (inputs.GameCount > GameCountSize && inputs.SubTabVisible)
                    {
                        // If there are sub tabs, it means we will have less space on the screen
                        // so we have to reduce the image height a bit
                        result -= 30;
                    }
                }

                Logger.Debug($"GameIconHeightConverter returning height of {result}");
                return result;
            }

            return LargeIconHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
