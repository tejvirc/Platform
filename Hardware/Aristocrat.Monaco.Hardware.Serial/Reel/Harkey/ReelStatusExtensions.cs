namespace Aristocrat.Monaco.Hardware.Serial.Reel.Harkey
{
    /// <summary>
    ///     Reel status extension methods
    /// </summary>
    public static class ReelStatusExtensions
    {
        /// <summary>
        ///     Returns true is the ReelStatus indicates the reel is connected
        /// </summary>
        /// <param name="status">The reel status</param>
        /// <returns>True is the reel is connected.</returns>
        public static bool IsReelConnected(this ReelStatus status)
        {
            return (status & (ReelStatus.RmsConnected | ReelStatus.RmConnected | ReelStatus.ReelSlowSpin)) > 0;
        }

        /// <summary>
        ///     Converts a ReelStatus enum to a GdsReelStatus
        /// </summary>
        /// <param name="status">The reel status</param>
        /// <param name="reelId">The reel id</param>
        /// <param name="initialized">If the reel is initialized</param>
        /// <param name="ignoreConnected">If the connected status should be ignored</param>
        /// <returns>A GDS reel status</returns>
        public static Contracts.Gds.Reel.GdsReelStatus ToGdsReelStatus(
            this ReelStatus status,
            int reelId,
            bool initialized,
            bool ignoreConnected)
        {
            return new Contracts.Gds.Reel.GdsReelStatus
            {
                ReelId = reelId,
                Connected = initialized && (status.IsReelConnected() || ignoreConnected),
                ReelStall = (status & ReelStatus.Stalled) == ReelStatus.Stalled,
                ReelTampered = (status & ReelStatus.ReelOutOfSync) == ReelStatus.ReelOutOfSync
            };
        }
    }
}