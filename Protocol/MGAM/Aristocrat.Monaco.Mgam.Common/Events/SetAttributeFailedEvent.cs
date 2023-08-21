namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Aristocrat.Mgam.Client.Messaging;
    using Kernel;

    /// <summary>
    ///     Published when Set Attribute Failed.
    /// </summary>
    public class SetAttributeFailedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetAttributeFailedEvent"/> class.
        /// </summary>
        /// <param name="response"></param>
        public SetAttributeFailedEvent(ServerResponseCode response)
        {
            Response = response;
        }

        /// <summary>
        ///     Gets the response code.
        /// </summary>
        public ServerResponseCode Response { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName} (Response: {Response})]";
        }
    }
}
