namespace Aristocrat.Monaco.G2S.UI.Converters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Localization.Properties;

    /// <summary>
    ///     Logging Levels Converter
    /// </summary>
    public class LoggingLevelsToLocalizedStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((SourceLevels)value)
            {
                case SourceLevels.All:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.G2SLoggingLevel_All);
                case SourceLevels.ActivityTracing:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.G2SLoggingLevel_ActivityTracing);
                case SourceLevels.Critical:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.G2SLoggingLevel_Critical);
                case SourceLevels.Error:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.G2SLoggingLevel_Error);
                case SourceLevels.Information:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.G2SLoggingLevel_Information);
                case SourceLevels.Off:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.G2SLoggingLevel_Off);
                case SourceLevels.Verbose:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.G2SLoggingLevel_Verbose);
                case SourceLevels.Warning:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.G2SLoggingLevel_Warning);
            }
            return string.Empty;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
