namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Designed to be used to bind int values to a group of radio buttons.
    ///     Xaml usage:
    ///     RadioButton IsChecked="{Binding DenomFilter,
    ///     Converter={StaticResource IntToBool},
    ///     ConverterParameter={StaticResource DenomA}}"
    ///     Converts to true if DenomFilter == DenomA, where
    ///     sys:Int32 x:Key="DenomA" 1 sys:Int32
    /// </summary>
    internal class IntToBoolConverter : IValueConverter
    {
        /// <summary>
        ///     Converts an int value to a bool
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">The value to compare against</param>
        /// <param name="culture">Not used</param>
        /// <returns>A bool indicating whether value equals parameter</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null && value.Equals(parameter);
        }

        /// <summary>
        ///     Convert a bool back into an int
        /// </summary>
        /// <param name="value">The bool value to check</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">The value to return if value is true</param>
        /// <param name="culture">Not used.</param>
        /// <returns>parameter is value is true. Binding.DoNothing otherwise</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // If we are true, convert to parameter
            return value != null && value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}