namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [Serializable]
    public class SubGameSerialization
    {
        [JsonProperty("gameType")]
        public string GameType { get; set; }

        [JsonProperty("SubGames")]
        public IList<SubGameInformation> SubGames { get; set; }
    }

    [Serializable]
    public class SubGameInformation
    {
        [JsonProperty("gameId")]
        public int GameId { get; set; }

        [JsonProperty("Denominations")]
        public IEnumerable<long> Denominations { get; set; }
    }
}
