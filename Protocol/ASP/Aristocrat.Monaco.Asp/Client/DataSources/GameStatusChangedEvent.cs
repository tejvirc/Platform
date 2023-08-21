namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Contracts;
    using Kernel;

    public class DacomGameStatusChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DacomGameStatusChangedEvent" /> class.
        /// </summary>
        /// <param name="gameEnableStatus">The new Game Enable Status</param>
        /// ///
        /// <param name="gameDisableReason">The new Game Disable Reason</param>
        public DacomGameStatusChangedEvent(GameEnableStatus gameEnableStatus, GameDisableReason gameDisableReason)
        {
            GameEnableStatus = gameEnableStatus;
            GameDisableReason = gameDisableReason;
        }

        public GameEnableStatus GameEnableStatus { get; }
        public GameDisableReason GameDisableReason { get; }
    }
}