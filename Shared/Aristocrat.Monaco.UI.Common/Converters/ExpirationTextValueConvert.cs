namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Windows.Data;

    /// <inheritdoc />
    public class ExpirationTextValueConvert : IValueConverter
    {
        private readonly string _daysFormatter;
        private readonly string _neverExpiresText;
        private readonly bool _displayText;

        /// <summary>
        ///     Creates the ExpirationTextValueConvert
        /// </summary>
        /// <param name="daysFormatter">The days formatter to use</param>
        /// <param name="neverExpiresText">The never expires text to add when the expiration is set to never expires</param>
        /// <param name="displayText"></param>
        public ExpirationTextValueConvert(string daysFormatter, string neverExpiresText, bool displayText)
        {
            _daysFormatter = daysFormatter;
            _neverExpiresText = neverExpiresText;
            _displayText = displayText;
        }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convert;

            if (value is int days && (int?)days > 0)
            {
                var daysFormat = _displayText ? string.Format(_daysFormatter, (int?)days) : ((int?)days).ToString();
                convert = Regex.Replace(daysFormat, "[^a-zA-Z0-9 ]", "");
            }
            else
            {
                convert = _displayText ? _neverExpiresText : string.Empty;
            }
           
            return convert;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}