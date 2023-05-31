namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using Contracts.InfoBar;

    [ValueConversion(typeof(InfoBarColor), typeof(Brush))]
    public class InfoBarColorToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (!(value is InfoBarColor color))
            {
                return Binding.DoNothing;
            }

            switch (color)
            {
                case InfoBarColor.Black:
                    return new SolidColorBrush(Colors.Black);

                case InfoBarColor.Brown:
                    return new SolidColorBrush(Colors.Brown);

                case InfoBarColor.Red:
                    return new SolidColorBrush(Colors.Red);

                case InfoBarColor.Orange:
                    return new SolidColorBrush(Colors.Orange);

                case InfoBarColor.Yellow:
                    return new SolidColorBrush(Colors.Yellow);

                case InfoBarColor.Green:
                    return new SolidColorBrush(Colors.Green);

                case InfoBarColor.Blue:
                    return new SolidColorBrush(Colors.Blue);

                case InfoBarColor.Violet:
                    return new SolidColorBrush(Colors.Violet);

                case InfoBarColor.Gray:
                    return new SolidColorBrush(Colors.Gray);

                case InfoBarColor.White:
                    return new SolidColorBrush(Colors.White);

                case InfoBarColor.Transparent:
                    return new SolidColorBrush(Colors.Transparent);

                default:
                    throw new ArgumentOutOfRangeException(nameof(color));
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}