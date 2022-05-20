namespace Aristocrat.Monaco.G2S.Common.Events
{
    /// <summary>
    ///     A HostUnreachableEvent posted whenever there are any transport state changes to
    ///     t_transportStates.G2S_hostUnreachable.
    /// </summary>
    public class HostUnreachableEvent : TransportEventBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostUnreachableEvent" /> class.
        /// </summary>
        /// <param name="hostId">The host identifier</param>
        public HostUnreachableEvent(int hostId)
            : base(hostId)
        {
        }
    }
}