namespace Aristocrat.Monaco.Application.Contracts
{
    using Kernel;

    /// <summary>
    ///     This event is published any time the network availablity changes after boot up
    /// </summary>
    public class NetworkAvailabilityChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NetworkAvailabilityChangedEvent" /> class.
        /// </summary>
        /// <param name="available">true if the network interface is now available</param>
        public NetworkAvailabilityChangedEvent(bool available)
        {
            Available = available;
        }

        /// <summary>
        ///     Gets a value indicating whether or not the network is now available
        /// </summary>
        public bool Available { get; }
    }
}