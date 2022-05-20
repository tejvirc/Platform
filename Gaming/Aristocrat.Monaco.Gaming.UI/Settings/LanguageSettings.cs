namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    /// <summary>
    ///     Language settings.
    /// </summary>
    internal class LanguageSettings
    {
        /// <summary>
        ///     Gets oer sets the player locale.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether multi-language is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the default language settings are active.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the language settings can be set to default.
        /// </summary>
        public bool IsDefaultable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the demo mode is enabled.
        /// </summary>
        public bool IsDemoModeEnabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the auto hold is configurable.
        /// </summary>
        public bool IsAutoHoldConfigurable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether player volume settings are adjustable.
        /// </summary>
        public bool IsPlayerVolumeAdjustable { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether service request is available.
        /// </summary>
        public bool IsServiceRequestAvailable { get; set; }
    }
}
