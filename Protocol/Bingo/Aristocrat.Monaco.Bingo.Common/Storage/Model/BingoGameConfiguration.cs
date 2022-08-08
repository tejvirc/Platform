namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [Serializable]
    public class BingoGameConfiguration
    {
        [JsonProperty("GameTitleId")]
        public string GameTitleId { get; set; }

        [JsonProperty("ThemeSkinId")]
        public string ThemeSkinId { get; set; }

        [JsonProperty("PaytableId")]
        public string PaytableId { get; set; }

        [JsonProperty("Denomination")]
        public long Denomination { get; set; }

        [JsonProperty("Bets")]
        public IReadOnlyCollection<long> Bets { get; set; } = Array.Empty<long>();

        [JsonProperty("QuickStopMode")]
        public bool QuickStopMode { get; set; }

        [JsonProperty("EvaluationTypePaytable")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaytableEvaluation EvaluationTypePaytable { get; set; }

        [JsonProperty("PlatformGameId")]
        public long PlatformGameId { get; set; }
    }
}