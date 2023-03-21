namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class GameIconWidthConverter : IValueConverter
    {
        private const GridUnitType DefaultGridUnitType = GridUnitType.Auto;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs && inputs.ExtraLargeIconLayout)
            {
                var scaleBy = inputs.GameWindowHeight / ScaleUtility.BaseScreenHeight;
                return inputs.GameWindowHeight > ScaleUtility.BaseScreenHeight
                    ? inputs.GameIconSize.Width * scaleBy
                    : inputs.GameIconSize.Width;
            }

            return DefaultGridUnitType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
