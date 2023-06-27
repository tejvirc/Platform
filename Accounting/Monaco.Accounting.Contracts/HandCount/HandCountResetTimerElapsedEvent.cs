namespace Aristocrat.Monaco.Accounting.Contracts.HandCount
{
    using Kernel;

    /// <summary>
    ///     Definition of the HandCountResetTimerElapsedEvent class
    /// </summary>
    public class HandCountResetTimerElapsedEvent : BaseEvent
    {
        /// <summary>
        /// Constructs the event with the residual amount to be reset
        /// </summary>
        /// <param name="residualAmount"></param>
        public HandCountResetTimerElapsedEvent(long residualAmount)
        {
            ResidualAmount = residualAmount;
        }
        /// <summary>
        /// The residual amount with the hand count to be reset
        /// </summary>
        public long ResidualAmount { get; set; }
    }
}
