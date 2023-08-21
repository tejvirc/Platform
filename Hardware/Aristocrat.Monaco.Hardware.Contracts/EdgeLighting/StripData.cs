namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    /// <summary>
    ///     The data for the light strips
    /// </summary>
    public class StripData
    {
        /// <summary>
        ///     The ID of the strip
        /// </summary>
        public int StripId { get; set; }

        /// <summary>
        ///     The number of LEDs on the strip
        /// </summary>
        public int LedCount { get; set; }
    }
}