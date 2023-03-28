namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    using System;
    using Newtonsoft.Json;

    [Serializable]
    public class BetInformationDetail
    {
        [JsonProperty("Bet")]
        public long Bet { get; set; }

        [JsonProperty("Rtp")]
        public decimal Rtp { get; set; }
    }
}