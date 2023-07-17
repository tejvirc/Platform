namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class GridSpacingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GameGridMarginInputs inputs)
            {
                var count = inputs.GameCount;
                if (inputs.TabView)
                {
                    if (inputs.ExtraLargeIconLayout)
                    {
                        return new Size(40, 0);
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
