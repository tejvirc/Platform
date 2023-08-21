namespace Aristocrat.Monaco.Gaming.UI.PlayerInfoDisplay
{
    using System;
    using Application.Contracts;
    using Contracts;
    using Contracts.PlayerInfoDisplay;
    using Kernel;

    /// <inheritdoc cref="IPlayerInfoDisplayFeatureProvider" />
    public sealed class PlayerInfoDisplayFeatureProvider : IPlayerInfoDisplayFeatureProvider
    {
        private readonly IPropertiesManager _propertiesManager;

        public PlayerInfoDisplayFeatureProvider(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            IsPlayerInfoDisplaySupported = propertiesManager.GetValue(GamingConstants.PlayerInformationDisplay.Enabled, true);
            IsGameInfoSupported = propertiesManager.GetValue(GamingConstants.PlayerInformationDisplay.PlayerInformationScreenEnabled, true);
            IsGameRulesSupported = propertiesManager.GetValue(GamingConstants.PlayerInformationDisplay.GameRulesScreenEnabled, true);
        }

        /// <inheritdoc />
        public bool IsPlayerInfoDisplaySupported { get; }

        /// <inheritdoc />
        public bool IsGameInfoSupported { get; }

        /// <inheritdoc />
        public bool IsGameRulesSupported { get; }

        /// <inheritdoc />
        public string ActiveLocaleCode => _propertiesManager.GetValue(ApplicationConstants.LocalizationPlayerCurrentCulture, ApplicationConstants.DefaultLanguage);

        /// <inheritdoc />
        public int TimeoutMilliseconds => _propertiesManager.GetValue(GamingConstants.PlayerInformationDisplay.TimeoutMilliseconds, GamingConstants.PlayerInformationDisplay.DefaultTimeoutMilliseconds);
    }
}