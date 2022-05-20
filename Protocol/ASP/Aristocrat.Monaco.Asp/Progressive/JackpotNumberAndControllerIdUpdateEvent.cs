namespace Aristocrat.Monaco.Asp.Progressive
{
    using Kernel;

    /// <summary>
    ///     An event that indicates request from link progressive controller to update jackpot number and controller id
    /// </summary>
    public class JackpotNumberAndControllerIdUpdateEvent : BaseEvent
    {
        /// <summary>
        /// The link progressive level this update applies to
        /// </summary>
        public int LevelId { get; }

        /// <summary>
        /// The jackpot number for this level
        /// </summary>
        public long JackpotNumber { get; }

        /// <summary>
        /// The jackpot controller id for this level
        /// </summary>
        public int JackpotControllerId { get; }

        public JackpotNumberAndControllerIdUpdateEvent(int levelId, long jackpotNumber, int jackpotControllerId)
        {
            LevelId = levelId;
            JackpotNumber = jackpotNumber;
            JackpotControllerId = jackpotControllerId;
        }
    }
}