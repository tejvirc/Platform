﻿namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Data;
    using log4net;

    public class GameIconImageHeightConverter : IValueConverter
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const double IconHeight = 200;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs)
            {
                var scaleBy = inputs.GameWindowHeight / ScaleUtility.BaseScreenHeight;

                // Lobby Icon ImageHeight. Affects the height of the icon art
                var iconHeight = inputs.ExtraLargeIconLayout ? inputs.GameIconSize.Height : IconHeight;
                var size = inputs.TabView
                    ? iconHeight * scaleBy
                    : double.NaN;

                if (inputs.GameCount > 8 && inputs.SubTabVisible)
                {
                    // If there are sub tabs, it means we will have less space on the screen
                    // so we have to reduce the image height a bit
                    size -= 30;
                }

                Logger.Debug($"GameIconImageHeightConverter returning height of {size}");
                return size;
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
