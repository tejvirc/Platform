﻿namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [Serializable]
    public class BingoGameConfiguration
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
        public IReadOnlyCollection<BetInformationDetail> BetInformationDetails { get; set; }

        [JsonProperty("QuickStopMode")]
        public bool QuickStopMode { get; set; }

        [JsonProperty("EvaluationTypePaytable")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaytableEvaluation EvaluationTypePaytable { get; set; }

        [JsonProperty("HelpUrl")]
        public string HelpUrl { get; set; }

        [JsonProperty("PlatformGameId")]
        public long PlatformGameId { get; set; }
    }
}