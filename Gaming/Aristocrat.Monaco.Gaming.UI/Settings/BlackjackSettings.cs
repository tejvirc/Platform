namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using Contracts.Models;
    using Newtonsoft.Json;

    /// <summary>
    ///     Blackjack settings.
    /// </summary>
    internal class BlackjackSettings : GameCategorySettings
    {
        [JsonIgnore]

        public override GameType GameType => GameType.Blackjack;
    }
}
