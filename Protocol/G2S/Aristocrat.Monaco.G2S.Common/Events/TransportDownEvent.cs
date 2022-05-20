namespace Aristocrat.Monaco.G2S.Common.Events
{
    /// <summary>
    ///     A TransportDownEvent posted whenever there are any transport state changes to t_transportStates.G2S_transportDown.
    /// </summary>
    public class TransportDownEvent : TransportEventBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportDownEvent" /> class.
        /// </summary>
        /// <param name="hostId">The host identifier</param>
        public TransportDownEvent(int hostId)
            : base(hostId)
        {
        }
    }
}