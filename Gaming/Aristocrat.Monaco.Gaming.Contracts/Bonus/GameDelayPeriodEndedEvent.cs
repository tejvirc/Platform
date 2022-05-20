namespace Aristocrat.Monaco.Gaming.Contracts.Bonus
{
    using Kernel;

    /// <summary>
    ///     Published when the game delay is no longer active
    /// </summary>
    public class GameDelayPeriodEndedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDelayPeriodEndedEvent" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier</param>
        public GameDelayPeriodEndedEvent(int deviceId)
        {
            DeviceId = deviceId;
        }

        /// <summary>
        ///     Gets the device identifier
        /// </summary>
        public int DeviceId { get; }
    }
}