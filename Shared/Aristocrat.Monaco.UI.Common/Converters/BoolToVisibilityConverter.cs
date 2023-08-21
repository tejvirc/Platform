namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Converts bool to visibility property.  By setting the TrueValue
    ///     and False value properties, the default behavior can be customized.
    ///     In particular, you can use this to invert the default behavior to
    ///     make something collapsed when true.
    ///     Xaml usage:
    ///     converters:BoolToVisibilityConverter x:Key="BoolToCollapsed" TrueValue="Collapsed" FalseValue="Visible"
    ///     Grid x:Name="GameTopScreenRoot" Visibility="{Binding IsLobbyVisible, Converter={StaticResource BoolToCollapsed}}"
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///     Gets or sets the true value
        /// </summary>
        public Visibility TrueValue { get; set; } = Visibility.Visible;

        /// <summary>
        ///     Gets or sets the false value
        /// </summary>
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        /// <summary>
        ///     Convert from bool to a visibility state
        /// </summary>
        /// <param name="value">the bool value to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a visibility state</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool == false)
            {
                return FalseValue;
            }

            return (bool)value ? TrueValue : FalseValue;
        }

        /// <summary>
        ///     Convert a visibility state back to bool
        /// </summary>
        /// <param name="value">the visibility state to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>true, false, or null if value wasn't a visibility state</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, TrueValue))
            {
                return true;
            }

            if (Equals(value, FalseValue))
            {
                return false;
            }

            return null;
        }
    }
}