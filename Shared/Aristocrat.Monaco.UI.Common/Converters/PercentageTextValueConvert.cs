namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <inheritdoc />
    public class PercentageTextValueConvert : IValueConverter
    {
        private readonly string _editingFormatter;
        private readonly string _displayFormatter;
        private readonly string _zeroPercentageText;
        private readonly bool _displayText;

        /// <summary>
        ///     Creates the PercentageTextValueConvert
        /// </summary>
        /// <param name="editingFormatter">The numerical formatter to use, e.g. {0.00} - sets the number of decimal places.</param>
        /// <param name="displayFormatter">The display formatter to use, e.g. {0.00}% - sets display when not editing.</param>
        /// <param name="zeroPercentageText">The optional zero percentage text to use when the value is zero.</param>
        /// <param name="displayText">Tells us whether we are currently editing or displaying</param>
        public PercentageTextValueConvert(string editingFormatter, string displayFormatter, string zeroPercentageText, bool displayText)
        {
            _editingFormatter = editingFormatter ?? "";
            _displayFormatter = displayFormatter ?? "";
            _zeroPercentageText = zeroPercentageText;
            _displayText = displayText;
        }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            if (value is decimal percentage)
            {
                if (percentage != 0)
                {
                    // Value is valid and not zero. Display as raw number if editing, or with symbols if not.
                    return _displayText ? string.Format(_displayFormatter, percentage)
                                        : string.Format(_editingFormatter, percentage);
                }
                else
                {
                    // Value is zero. When editing, we need to show nothing, making numerical entry easier.
                    if (!_displayText) return string.Empty;

                    // If not editing, then use our special zero percentage string if that's been provided.
                    if  (!string.IsNullOrEmpty(_zeroPercentageText)) return _zeroPercentageText;

                    // Otherwise, just display the result of formatting the value of zero, as normal.
                    return string.Format(_displayFormatter, percentage);
                }
            }
            else
            {
                return string.Empty;
            }
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}