namespace Aristocrat.Monaco.G2S.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Base class for a transport related event
    /// </summary>
    public abstract class TransportEventBase : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportEventBase" /> class.
        /// </summary>
        /// <param name="hostId">The host id.</param>
        protected TransportEventBase(int hostId)
        {
            HostId = hostId;
        }

        /// <summary>
        ///     Gets the host id whose transport state has changed.
        /// </summary>
        public int HostId { get; }
    }
}