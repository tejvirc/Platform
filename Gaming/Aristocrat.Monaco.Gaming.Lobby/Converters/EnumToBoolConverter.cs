namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Designed to be used to bind Enums to a group of radio buttons.
    ///     Xaml usage:
    ///     RadioButton IsChecked="{Binding GameFilter,
    ///     Converter={StaticResource EnumToBool},
    ///     ConverterParameter={StaticResource PokerGameType}}"
    ///     Converts to true if GameFilter == PokerGameType, where
    ///     model:GameType x:Key="PokerGameType"PokerModel:GameType
    ///     ///
    /// </summary>
    internal class EnumToBoolConverter : IValueConverter
    {
        /// <summary>
        ///     Converts the enumerable to a bool
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">The parameter to compare to</param>
        /// <param name="culture">The culture for the comparision. Not used</param>
        /// <returns>A bool indicating that value equals parameter</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // EnumValue == EnumParameter, return true.
            return value != null && value.Equals(parameter);
        }

        /// <summary>
        ///     Converts a bool back to an enum.
        /// </summary>
        /// <param name="value">The bool to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">The parameter to return if value is true</param>
        /// <param name="culture">Not used</param>
        /// <returns>The converted value</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // If we are true, convert to EnumParameter
            return value != null && value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}