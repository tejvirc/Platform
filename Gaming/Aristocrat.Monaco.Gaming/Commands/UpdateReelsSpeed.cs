namespace Aristocrat.Monaco.Gaming.Commands
{
    using ReelSpeedData = Hardware.Contracts.Reel.ControlData.ReelSpeedData;

    /// <summary>
    ///     Set the reel speed for the mechanical reels
    /// </summary>
    public class UpdateReelsSpeed
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateReelsSpeed" /> class.
        /// </summary>
        /// <param name="speedData">The reel speed data</param>
        public UpdateReelsSpeed(params ReelSpeedData[] speedData)
        {
            SpeedData = speedData;
        }

        /// <summary>
        ///     Gets the reel speed data
        /// </summary>
        public ReelSpeedData[] SpeedData { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the reel speed was changed
        /// </summary>
        public bool Success { get; set; }
    }
}