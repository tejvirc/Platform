﻿namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Models;

    public class GameIconImageWidthConverter : IValueConverter
    {
        private const double LargeScreenSize = 1080;
        private const double LargeScreenScale = 2.0;
        private const GridUnitType DefaultGridUnitType = GridUnitType.Auto;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs && inputs.ExtraLargeIconLayout)
            {
                return inputs.ScreenHeight > LargeScreenSize
                    ? inputs.GameIconSize.Width * LargeScreenScale
                    : inputs.GameIconSize.Width;
            }

            return DefaultGridUnitType;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}