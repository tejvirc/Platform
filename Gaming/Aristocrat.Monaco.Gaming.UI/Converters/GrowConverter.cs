namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Definition of the GrowConverter class
    /// </summary>
    /// <remarks>
    ///     Used for maintaining a fixed width on ComboBox controls
    /// </remarks>
    public class GrowConverter : IValueConverter
    {
        /// <summary>
        ///     Gets Maximum value so far
        /// </summary>
        public double Maximum { get; private set; }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dValue = value is double d ? d : 0.0d;
            if (dValue > Maximum)
            {
                Maximum = dValue;
            }
            else if (dValue < Maximum)
            {
                dValue = Maximum;
            }

            return dValue;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
