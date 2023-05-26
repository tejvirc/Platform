namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    /// <summary>
    ///     The reel status class.
    /// </summary>
    public class ReelStatus
    {
        /// <summary>
        ///     Gets or sets the reel id for the status
        /// </summary>
        public int ReelId { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the reel has stalled
        /// </summary>
        public bool ReelStall { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the reel has been tampered with
        /// </summary>
        public bool ReelTampered { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the reel is connected
        /// </summary>
        public bool Connected { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a reel requested to spin/nudge to goal resulted in low voltage.
        /// </summary>
        public bool LowVoltage { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not a reel requested to home resulted in an error.
        /// </summary>
        public bool FailedHome { get; set; }
    }
}
