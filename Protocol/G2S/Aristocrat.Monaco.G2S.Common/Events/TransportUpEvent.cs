namespace Aristocrat.Monaco.G2S.Common.Events
{
    /// <summary>
    ///     A TransportUpEvent posted whenever there are any transport state changes to t_transportStates.G2S_transportUp.
    /// </summary>
    public class TransportUpEvent : TransportEventBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportUpEvent" /> class.
        /// </summary>
        /// <param name="hostId">The host identifier</param>
        public TransportUpEvent(int hostId)
            : base(hostId)
        {
        }
    }
}