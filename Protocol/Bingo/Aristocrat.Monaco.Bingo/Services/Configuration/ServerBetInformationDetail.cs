namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using Newtonsoft.Json;

    public class ServerBetInformationDetail
    {
        [JsonProperty("Bet")]
        public long Bet { get; set; }

        [JsonProperty("Rtp")]
        public decimal Rtp { get; set; }
    }
}