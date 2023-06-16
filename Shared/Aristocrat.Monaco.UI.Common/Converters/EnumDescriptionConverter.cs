namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Extensions;

    /// <summary>
    ///     Convert an enum value to its corresponding Description attribute string
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        /// <summary>
        ///     Convert an enum value to its corresponding Description attribute string
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Enum e)
            {
                var nonLocalizedValue = e.GetDescription(e.GetType()) ?? e.ToString();
                var displayValue = Localizer.For(CultureFor.Operator).GetString(e.ToString(), _ => { }) ?? nonLocalizedValue;
                return displayValue;
            }

            return string.Empty;
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
