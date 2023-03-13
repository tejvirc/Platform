namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System;
    using System.Collections.Generic;
    using Common.Storage.Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class ServerGameConfiguration
    {
        [JsonProperty("GameTitleId")]
        public long GameTitleId { get; set; }

        [JsonProperty("ThemeSkinId")]
        public long ThemeSkinId { get; set; }

        [JsonProperty("PaytableId")]
        public long PaytableId { get; set; }

        [JsonProperty("Denomination")]
        public long Denomination { get; set; }

        [JsonProperty("BetInformation")]
        public IReadOnlyCollection<ServerBetInformationDetail> BetInformationDetails { get; set; }

        [JsonProperty("Progressive")]
        public int Progressive { get; set; }

        [JsonProperty("QuickStopMode")]
        public bool QuickStopMode { get; set; }

        [JsonProperty("EvaluationTypePaytable")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaytableEvaluation EvaluationTypePaytable { get; set; }

        [JsonProperty("HelpUrl")]
        public string HelpUrl { get; set; }

        [JsonProperty("CrossGameProgressiveEnabled")]
        public bool CrossGameProgressiveEnabled { get; set; }

        [JsonProperty("SideBetGames")]
        public IReadOnlyCollection<SideBetGameConfiguration> SideBetGames { get; set; }
    }
}