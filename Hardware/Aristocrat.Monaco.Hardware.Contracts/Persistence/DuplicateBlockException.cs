namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the DuplicateBlockException class.
    /// </summary>
    [Serializable]
    public class DuplicateBlockException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateBlockException" /> class.
        /// </summary>
        public DuplicateBlockException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateBlockException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public DuplicateBlockException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateBlockException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public DuplicateBlockException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DuplicateBlockException" /> class.
        /// </summary>
        /// <param name="info">Information on how to serialize an ServiceException.</param>
        /// <param name="context">Information on the streaming context for an ServiceException.</param>
        protected DuplicateBlockException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}