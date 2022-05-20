namespace Aristocrat.Monaco.Application.Drm
{
    using Newtonsoft.Json;

    internal class TokenData
    {
        /// <summary>
        ///     Gets or sets the jurisdictionId associated with the token
        /// </summary>
        [JsonProperty("jurisdictionId")]
        public string JurisdictionId { get; set; }
    }
}