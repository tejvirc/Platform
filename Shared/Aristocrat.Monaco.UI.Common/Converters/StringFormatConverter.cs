namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>
    ///     Converts an array of objects to a formatted string.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : MarkupExtension, IMultiValueConverter
    {
        /// <summary>
        ///     Gets or sets the string format.
        /// </summary>
        public string StringFormat { get; set; } = "{0}";

        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(StringFormat, values);
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new[] { value };
        }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
