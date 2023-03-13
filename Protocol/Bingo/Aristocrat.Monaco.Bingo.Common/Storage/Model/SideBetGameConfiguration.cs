namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [Serializable]
    public class SideBetGameConfiguration
    {
        [JsonProperty("GameTitleId")]
        public long GameTitleId { get; set; }

        [JsonProperty("PaytableId")]
        public long PaytableId { get; set; }

        [JsonProperty("Denomination")]
        public long Denomination { get; set; }

        [JsonProperty("BetInformation")]
        public IReadOnlyCollection<BetInformationDetail> BetInformation { get; set; } = Array.Empty<BetInformationDetail>();

        [JsonProperty("EvaluationTypePaytable")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaytableEvaluation EvaluationTypePaytable { get; set; }

        [JsonProperty("CrossGameProgressiveEnabled")]
        public bool CrossGameProgressiveEnabled { get; set; }

        [JsonProperty("Progressive")]
        public int Progressive { get; set; }
    }
}