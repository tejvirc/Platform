namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event when Live Authentication fails
    /// </summary>
    [ProtoContract]
    public class LiveAuthenticationFailedEvent : BaseEvent
    {

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public LiveAuthenticationFailedEvent()
        {
        }

        /// <summary>
        ///     Construct a <see cref="LiveAuthenticationFailedEvent"/>.
        /// </summary>
        /// <param name="message">Message.</param>
        public LiveAuthenticationFailedEvent(string message)
        {
            Message = message;
        }

        /// <summary>
        ///     Get the message.
        /// </summary>
        [ProtoMember(1)]
        public string Message { get; }
    }
}