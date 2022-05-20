namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using Contracts.Models;
    using Newtonsoft.Json;

    /// <summary>
    ///     Keno settings.
    /// </summary>
    internal class KenoSettings : GameCategorySettings
    {
        /// <inheritdoc />
        [JsonIgnore]
        public override GameType GameType => GameType.Keno;
    }
}
