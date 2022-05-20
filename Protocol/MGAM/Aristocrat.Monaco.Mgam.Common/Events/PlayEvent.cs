namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Published when the Play command is received.
    /// </summary>
    public class PlayEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayEvent"/> class.
        /// </summary>
        public PlayEvent()
        {
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="sessionId">The identifier of the session that was active.</param>
        public PlayEvent(int sessionId)
        {
            SessionId = sessionId;
        }

        /// <summary>
        ///     Gets the existing session ID.
        /// </summary>
        public int? SessionId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName} (SessionId: {SessionId})]";
        }
    }
}
