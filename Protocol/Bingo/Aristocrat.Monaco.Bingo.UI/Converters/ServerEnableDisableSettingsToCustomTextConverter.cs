namespace Aristocrat.Monaco.Bingo.UI.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    /// Converts an on/off, enable/disable, true/false setting to custom resources.
    /// </summary>
    public class ServerEnableDisableSettingsToCustomTextConverter : IValueConverter
    {
        private static readonly HashSet<string> TrueValues = new(StringComparer.OrdinalIgnoreCase)
        {
            "1", "enable", "enabled", "on", "true"
        };

        private static readonly HashSet<string> FalseValues = new(StringComparer.OrdinalIgnoreCase)
        {
            "0", "disable", "disabled", "off", "false"
        };
        
        private readonly ILocalizer _localizer;

        /// <summary>
        ///     Instantiates a new ServerEnableDisableSettingsToCustomTextConverter 
        /// </summary>
        public ServerEnableDisableSettingsToCustomTextConverter()
            : this(Localizer.For(CultureFor.Operator))
        {
        }

        /// <summary>
        ///     Instantiates a new ServerEnableDisableSettingsToCustomTextConverter 
        /// </summary>
        /// <param name="localizer">The localization to use for this converter</param>
        public ServerEnableDisableSettingsToCustomTextConverter(ILocalizer localizer)
        {
            _localizer = localizer ?? Localizer.For(CultureFor.Operator);
        }

        /// <summary>
        ///     The resource key to use when the setting is ON.
        /// </summary>
        public string EnabledResourceKey { get; set; } = ResourceKeys.Enabled;

        /// <summary>
        ///     The resource key to use when the setting is OFF.
        /// </summary>
        public string DisabledResourceKey { get; set; } = ResourceKeys.Disabled;

        /// <summary>
        ///     Convert the server setting to the preferred verbiage.
        /// </summary>
        /// <param name="value">the server setting to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a visibility state</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string s)
            {
                return string.Empty;
            }

            if (TrueValues.Contains(s))
            {
                return _localizer.GetString(EnabledResourceKey);
            }

            if (FalseValues.Contains(s))
            {
                return _localizer.GetString(DisabledResourceKey);
            }

            return value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
