namespace Aristocrat.Monaco.Hardware.Contracts.TicketContent
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the PropertyNotSetException class.
    /// </summary>
    [Serializable]
    public class PropertyNotSetException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotSetException" /> class.
        /// </summary>
        public PropertyNotSetException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotSetException" /> class.
        /// </summary>
        /// <param name="message">The exception message. </param>
        public PropertyNotSetException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotSetException" /> class.
        /// </summary>
        /// <param name="message">The exception message. </param>
        /// <param name="inner">The inner exception. </param>
        public PropertyNotSetException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyNotSetException" /> class.
        /// </summary>
        /// <param name="info">The exception info. </param>
        /// <param name="context">The exception context. </param>
        protected PropertyNotSetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}