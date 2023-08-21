namespace Aristocrat.Monaco.Application.UI.Settings
{
    using Contracts.Localization;

    /// <summary>
    ///     Edge light settings.
    /// </summary>
    internal sealed class EdgeLightSettings : SettingsBase
    {
        public EdgeLightSettings()
        {
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, RefreshAllDisplayableSettings);
        }

        ~EdgeLightSettings()
        {
            EventBus.UnsubscribeAll(this);
        }
        /// <summary>
        ///     Gets or sets the maximum allowed edge lighting brightness.
        /// </summary>
        public int MaximumAllowedEdgeLightingBrightness { get; set; }

        /// <summary>
        ///     Gets or sets the lighting override color selection.
        /// </summary>
        public string LightingOverrideColorSelection { get; set; }

        /// <summary>
        ///     Gets or sets if the bottom edge lighting is enabled.
        /// </summary>
        public bool BottomEdgeLightingEnabled { get; set; }
    }
}
