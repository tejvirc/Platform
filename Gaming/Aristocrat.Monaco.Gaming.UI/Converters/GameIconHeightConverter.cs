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
        private const int GameHeightWithSubTabAdjustment = 30;

        // Extra height for denom button panel on ExtraLargeIconLayout tabs
        private const double DenomPanelHeight = 85;

        // Extra height for individual Major jackpot banner on ExtraLargeIconLayout tabs
        private const double MajorJackpotHeight = 30;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs)
            {
                double result;
                if (inputs.ExtraLargeIconLayout)
                {
                    result = DenomPanelHeight + inputs.GameIconSize.Height;
                    if (!inputs.MultipleGameAssociatedSapLevelTwoEnabled)
                    {
                        result += MajorJackpotHeight;
                    }
                }
                else
                {
                    // Lobby layout icon sizes based on number of games. Affects the size of the icon image
                    var size = inputs.GameCount > GameCountSize || inputs.TabView ? SmallIconHeight : LargeIconHeight;
                    result = size;

                    if (inputs.GameCount > GameCountSize && inputs.SubTabVisible)
                    {
                        // If there are sub tabs, it means we will have less space on the screen
                        // so we have to reduce the image height a bit
                        result -= GameHeightWithSubTabAdjustment;
                    }
                }

                Logger.Debug($"GameIconHeightConverter returning height of {result}");
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
