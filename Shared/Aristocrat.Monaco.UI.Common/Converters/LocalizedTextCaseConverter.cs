namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Application.Contracts.Localization;

    /// <summary>
    ///     Convert text to parameter case after localization
    /// </summary>
    public class LocalizedTextCaseConverter : IValueConverter
    {
        private readonly TextCaseConverter _textCaseConverter = new TextCaseConverter();

        /// <summary>
        ///     Convert value text to parameter case after localization
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var locString = Localizer.For(CultureFor.Operator).GetString(value.ToString());

            return _textCaseConverter.Convert(locString, locString.GetType(), parameter, culture);
        }

        /// <summary>
        ///     Not used
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}