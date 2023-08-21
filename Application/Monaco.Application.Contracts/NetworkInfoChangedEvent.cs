namespace Aristocrat.Monaco.Application.Contracts
{
    using Kernel;

    /// <summary>
    ///     This event is published any time the network information changes
    /// </summary>
    public class NetworkInfoChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NetworkInfoChangedEvent" /> class.
        /// </summary>
        /// <param name="networkInfo">The new network information</param>
        public NetworkInfoChangedEvent(NetworkInfo networkInfo)
        {
            NetworkInfo = networkInfo;
        }

        /// <summary>
        ///     Gets the new network information
        /// </summary>
        public NetworkInfo NetworkInfo { get; }
    }
}