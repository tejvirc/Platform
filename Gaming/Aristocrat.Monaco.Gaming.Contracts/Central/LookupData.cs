namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     Contains the data which will be converted to JSON and
    ///     sent to the runtime in the Outcome class as the LookupData property
    /// </summary>
    public class LookupData
    {
        /// <summary>
        ///     The version of this data format
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        ///     The award id. The index to use for presentation selection.
        /// </summary>
        public int AwardId { get; set; } = 0;

        /// <summary>
        ///     The progressives flags, volatility flags, etc.
        /// </summary>
        public string Flags { get; set; } = string.Empty;

        /// <summary>
        ///     comma delimited progressive level(s) won
        /// </summary>
        public string Progressives { get; set; } = string.Empty;
    }
}