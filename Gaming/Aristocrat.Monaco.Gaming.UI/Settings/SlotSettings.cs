namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using Contracts;
    using Contracts.Models;
    using Newtonsoft.Json;

    /// <summary>
    ///     Slot settings.
    /// </summary>
    internal class SlotSettings : GameCategorySettings
    {
        private PlayMode _continuousPlayMode;
        private bool _reelStopEnabled;

        /// <inheritdoc />
        [JsonIgnore]
        public override GameType GameType => GameType.Slot;

        /// <summary>
        ///     Gets or sets continuations play mode.
        /// </summary>
        public PlayMode ContinuousPlayMode
        {
            get => _continuousPlayMode;

            set => SetProperty(ref _continuousPlayMode, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether real stop is enabled.
        /// </summary>
        public bool ReelStopEnabled
        {
            get => _reelStopEnabled;

            set => SetProperty(ref _reelStopEnabled, value);
        }
    }
}
