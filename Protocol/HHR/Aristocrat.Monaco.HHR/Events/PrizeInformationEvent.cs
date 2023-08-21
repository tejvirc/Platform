namespace Aristocrat.Monaco.Hhr.Events
{
    using Kernel;
    using Services;

    /// <summary>
    ///     This event will be fired when PrizeInformation will be available.
    /// </summary>
    public class PrizeInformationEvent : BaseEvent
    {
        public PrizeInformationEvent(PrizeInformation prizeInformation)
        {
            PrizeInformation = prizeInformation;
        }

        /// <summary>
        ///     PrizeInformation associated with the event
        /// </summary>
        public PrizeInformation PrizeInformation { get; }
    }
}
