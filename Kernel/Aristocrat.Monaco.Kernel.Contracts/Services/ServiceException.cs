namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     ServiceException is the exception raised when a method of IServiceManager fails.
    /// </summary>
    [Serializable]
    public class ServiceException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceException" /> class.
        /// </summary>
        public ServiceException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceException" /> class.  Also contains an error information
        ///     message.
        /// </summary>
        /// <param name="message">Associated error information for ServiceException.</param>
        public ServiceException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceException" /> class.  Also contains an error information
        ///     message and InnerException.
        /// </summary>
        /// <param name="message">Associated error information for ServiceException.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public ServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceException" /> class. Also contains SerializationInfo and
        ///     StreamingContext.
        /// </summary>
        /// <param name="info">Information on how to serialize an ServiceException.</param>
        /// <param name="context">Information on the streaming context for an ServiceException.</param>
        protected ServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}