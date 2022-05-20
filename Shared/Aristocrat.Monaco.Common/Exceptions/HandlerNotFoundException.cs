namespace Aristocrat.Monaco.Common.Exceptions
{
    using System;

    /// <summary>
    ///     Exception thrown when ICommandHandler registered for IHandlerConnector is not found
    /// </summary>
    [Serializable]
    public class HandlerNotFoundException<T> : Exception
    {
        /// <summary>
        ///     Initializes a new instance of HandlerNotFoundException class with default message
        /// </summary>
        public HandlerNotFoundException()
            : base($"This IHandlerConnector expects a handler of type {typeof(T)}. No handler found.")
        {
        }

        /// <summary>
        ///     Initializes a new instance of HandlerNotFoundException class with specified message
        /// </summary>
        /// <param name="message">Exception message</param>
        public HandlerNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of HandlerNotFoundException class with specified message and inner exception
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public HandlerNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}