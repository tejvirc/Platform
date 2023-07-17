namespace Aristocrat.Monaco.Gaming.Commands
{
    using Hardware.Contracts.Reel.ControlData;

    /// <summary>
    ///     The PrepareStopReels class
    /// </summary>
    public class PrepareStopReels
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="PrepareStopReels"/> class.
        /// </summary>
        /// <param name="reelStopData">The reel stop data</param>
        public PrepareStopReels(params ReelStopData[] reelStopData)
        {
            ReelStopData = reelStopData;
        }

        /// <summary>
        ///     Gets the reel stop data
        /// </summary>
        public ReelStopData[] ReelStopData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the stop was prepared
        /// </summary>
        public bool Success { get; set; }
    }
}
