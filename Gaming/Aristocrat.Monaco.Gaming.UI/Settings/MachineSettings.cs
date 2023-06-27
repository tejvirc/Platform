﻿namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using Application.Contracts.Localization;
    using Application.UI.Settings;

    public class MachineSettings : SettingsBase
    {
        public MachineSettings()
        {
            EventBus.Subscribe<OperatorCultureChangedEvent>(this, RefreshAllDisplayableSettings);
        }

        ~MachineSettings()
        {
            EventBus.UnsubscribeAll(this);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the game category settings are applied.
        /// </summary>
        public bool ApplyGameCategorySettings { get; set; }

        /// <summary>
        ///     Gets or sets the idle text.
        /// </summary>
        public string IdleText { get; set; }

        /// <summary>
        ///     Gets or sets the idle time period.
        /// </summary>
        public int IdleTimePeriod { get; set; }

        /// <summary>
        ///     Gets or sets the reel duration.
        /// </summary>
        public int GameRoundDurationMs { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether the reel stop is configured.
        /// </summary>
        public bool ReelStopConfigured { get; set; }
    }
}