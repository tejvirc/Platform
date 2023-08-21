namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when the VLT needs to re-discover and reconnect to VLT Service.
    /// </summary>
    public class ForceDisconnectEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ForceDisconnectEvent"/> class.
        /// </summary>
        /// <param name="reason"></param>
        public ForceDisconnectEvent(DisconnectReason reason)
        {
            Reason = reason;
        }

        /// <summary>
        ///     Gets the disconnect reason.
        /// </summary>
        public DisconnectReason Reason { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName} (Reason: {Reason})]";
        }
    }
}
