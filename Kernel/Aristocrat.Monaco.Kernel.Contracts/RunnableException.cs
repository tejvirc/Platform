namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the RunnableException class.  This exception should be
    ///     thrown whenever an IRunnable encounters an error.
    /// </summary>
    [Serializable]
    public class RunnableException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RunnableException" /> class.
        /// </summary>
        public RunnableException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RunnableException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public RunnableException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RunnableException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public RunnableException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RunnableException" /> class.
        /// </summary>
        /// <param name="info">Information on how to serialize a RunnableException.</param>
        /// <param name="context">Information on the streaming context for a RunnableException.</param>
        protected RunnableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}