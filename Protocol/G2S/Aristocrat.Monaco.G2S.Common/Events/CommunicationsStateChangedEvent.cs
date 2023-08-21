namespace Aristocrat.Monaco.G2S.Common.Events
{
    using Kernel;

    /// <summary>
    ///     The <c>CommunicationsStateChangedEvent</c> is posted when the communications state changes for the specified host
    ///     Id
    /// </summary>
    public class CommunicationsStateChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommunicationsStateChangedEvent" /> class.
        /// </summary>
        /// <param name="hostId">The host identifier</param>
        /// <param name="online">true if the host is online</param>
        public CommunicationsStateChangedEvent(int hostId, bool online)
        {
            HostId = hostId;
            Online = online;
        }

        /// <summary>
        ///     Gets the host identifier
        /// </summary>
        public int HostId { get; }

        /// <summary>
        ///     Gets a value indicating whether the host is online
        /// </summary>
        public bool Online { get; }
    }
}