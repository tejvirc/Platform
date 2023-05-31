namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Models;

    public class GridSpacingConverter : IValueConverter
    {
        private readonly double _scaleBy = ScaleUtility.GetScale();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs)
            {
                var count = inputs.GameCount;
                if (inputs.TabView)
                {
                    if (inputs.ExtraLargeIconLayout)
                    {
                        if (inputs.GameCount == 1)
                        {
                            return new Size();
                        }

                        const double borderWidth = 40;
                        // Calculate spacing for 2 icons - note this is the spacing for the major banner, game icon, and denom panel all together
                        var midSpacing = ((ScaleUtility.BaseScreenWidth - borderWidth * 2) - (count * inputs.GameIconSize.Width)) / 3.0;
                        midSpacing = midSpacing >= 0 ? midSpacing : 0;

                        return new Size(midSpacing * _scaleBy, 0);
                    }
                    return count == 9 ? new Size(200, 0) : new Size(40, 0);
                }

                return count <= 8
                    ? new Size(40, 50)
                    : count == 9
                        ? new Size(130, 18)
                        : count <= 15
                            ? new Size(40, 20)
                            : count <= 18
                                ? new Size(20, 50)
                                : count <= 21
                                    ? new Size(20, 80)
                                    : count <= 36
                                        ? new Size(20, 50)
                                        : new Size(20, 80);
            }

            return new Size(40, 20);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
