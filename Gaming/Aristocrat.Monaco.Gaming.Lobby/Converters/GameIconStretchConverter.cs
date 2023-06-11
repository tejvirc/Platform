namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using Models;

    public class GameIconStretchConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Stretch.Uniform;
            }

            if (value is GameGridMarginInputs inputs)
            {
                // Lobby Icon Stretch mode
                return inputs.TabView ? Stretch.None : Stretch.Uniform;
            }

            return Stretch.Uniform;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
