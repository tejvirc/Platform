namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using Properties;

    /// <summary>
    ///     The disconnected event for a given reel controller
    /// </summary>
    public class DisconnectedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisconnectedEvent" /> class.
        /// </summary>
        public DisconnectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisconnectedEvent" /> class.
        /// </summary>
        /// <param name="reelControllerId">The associated reel controller ID.</param>
        public DisconnectedEvent(int reelControllerId)
            : base(reelControllerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.ReelControllerText} {Resources.CommunicationFailureText}";
        }
    }
}