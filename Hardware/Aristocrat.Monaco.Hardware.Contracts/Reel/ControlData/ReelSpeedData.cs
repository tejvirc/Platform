namespace Aristocrat.Monaco.Hardware.Contracts.Reel.ControlData
{
    /// <summary>
    ///     The data needed for setting the reel speed on the controller
    /// </summary>
    public class ReelSpeedData
    {
        /// <summary>
        ///     Creates the reel speed data
        /// </summary>
        /// <param name="reelId">The reelId to set the speed of</param>
        /// <param name="speed">The speed to set the reel to</param>
        public ReelSpeedData(int reelId, int speed)
        {
            ReelId = reelId;
            Speed = speed;
        }

        /// <summary>
        ///     Gets the reel Id
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the reel speed
        /// </summary>
        public int Speed { get; }
    }
}