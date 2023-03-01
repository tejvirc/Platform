namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Used to apply a scale factor to a binding.
    ///     Xaml usage:
    ///     RadioButton IsChecked="{Binding GameFilter,
    ///     Converter={StaticResource EnumToBool},
    ///     ConverterParameter={StaticResource PokerGameType}}"
    ///     Converts to true if GameFilter == PokerGameType, where
    ///     model:GameType x:Key="PokerGameType">Poker model:GameType>
    /// </summary>
    public class ScaleConverter : IValueConverter
    {
        /// <summary>
        ///     Convert an object by scaling it
        /// </summary>
        /// <param name="value">The value to scale</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">the scaling factor</param>
        /// <param name="culture">The culture for the numbers. not used</param>
        /// <returns>The scaled object</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return 0;
            }

            var scale = 1.0;
            if (double.TryParse(value.ToString(), out var num))
            {
                double.TryParse(parameter.ToString(), out scale);
            }

            return num * scale;
        }

        /// <summary>
        ///     Convert an object back to its original value
        /// </summary>
        /// <param name="value">the object to scale</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">The scale factor</param>
        /// <param name="culture">The culture. Not used</param>
        /// <returns>The converted object</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}