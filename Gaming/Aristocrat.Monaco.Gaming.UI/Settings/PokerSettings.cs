namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using Contracts.Models;
    using Newtonsoft.Json;

    /// <summary>
    ///     Poker settings.
    /// </summary>
    internal class PokerSettings : GameCategorySettings
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override GameType GameType => GameType.Poker;
    }
}
