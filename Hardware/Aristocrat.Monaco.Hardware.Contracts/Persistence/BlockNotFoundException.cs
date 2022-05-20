namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Definition of the NonExistentBlockException class.
    /// </summary>
    [Serializable]
    public class BlockNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockNotFoundException" /> class.
        /// </summary>
        public BlockNotFoundException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockNotFoundException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public BlockNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockNotFoundException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public BlockNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockNotFoundException" /> class.
        /// </summary>
        /// <param name="info">Information on how to serialize an ServiceException.</param>
        /// <param name="context">Information on the streaming context for an ServiceException.</param>
        protected BlockNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}