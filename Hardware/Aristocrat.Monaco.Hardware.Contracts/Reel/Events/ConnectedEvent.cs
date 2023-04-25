namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using Properties;

    /// <summary>
    ///     The connected event for a given reel controller
    /// </summary>
    public class ConnectedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectedEvent" /> class.
        /// </summary>
        public ConnectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectedEvent" /> class.
        /// </summary>
        /// <param name="reelControllerId">The associated reel controller ID.</param>
        public ConnectedEvent(int reelControllerId)
            : base(reelControllerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.ReelControllerText} {Resources.CommunicationFailureText} {Resources.ClearedText}";
        }
    }
}