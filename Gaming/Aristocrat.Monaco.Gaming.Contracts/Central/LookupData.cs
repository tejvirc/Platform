namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using Newtonsoft.Json;

    /// <summary>
    ///     Contains the data which will be converted to JSON and
    ///     sent to the runtime in the Outcome class as the LookupData property
    /// </summary>
    public class LookupData
    {
        /// <summary>
        ///     The version of this data format
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        ///     The award id. The index to use for presentation selection.
        /// </summary>
        [JsonProperty(PropertyName = "awardId")]
        public int AwardId { get; set; } = 0;

        /// <summary>
        ///     The progressives flags, volatility flags, etc.
        /// </summary>
        [JsonProperty(PropertyName = "flags")]
        public string Flags { get; set; } = string.Empty;

        /// <summary>
        ///     comma delimited progressive level(s) won
        /// </summary>
        [JsonProperty(PropertyName = "progressives")]
        public string Progressives { get; set; } = string.Empty;
    }
}