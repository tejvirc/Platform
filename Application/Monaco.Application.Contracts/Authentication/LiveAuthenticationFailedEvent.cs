namespace Aristocrat.Monaco.Application.Contracts.Authentication
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event when Live Authentication fails
    /// </summary>
    [Serializable]
    public class LiveAuthenticationFailedEvent : BaseEvent
    {
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
        public string Message { get; }
    }
}