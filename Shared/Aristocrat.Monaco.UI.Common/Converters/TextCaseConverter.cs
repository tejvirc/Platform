namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    ///     Convert text to parameter case
    /// </summary>
    public class TextCaseConverter : IValueConverter
    {
        /// <summary>
        ///     Convert value text to parameter case
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (parameter != null && Enum.TryParse<CharacterCasing>(parameter.ToString(), out var casing))
            {
                if (casing == CharacterCasing.Upper)
                {
                    return value.ToString().ToUpper();
                }

                if (casing == CharacterCasing.Lower)
                {
                    return value.ToString().ToLower();
                }
            }

            return value.ToString();
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
