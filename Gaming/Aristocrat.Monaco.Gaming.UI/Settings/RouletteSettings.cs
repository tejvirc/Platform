namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using Contracts.Models;
    using Newtonsoft.Json;

    /// <summary>
    ///     Roulette settings.
    /// </summary>
    internal class RouletteSettings : GameCategorySettings
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override GameType GameType => GameType.Roulette;
    }
}
