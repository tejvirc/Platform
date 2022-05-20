namespace Aristocrat.Monaco.Hardware.Contracts.Persistence
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Block field not found exception
    /// </summary>
    [Serializable]
    public class BlockFieldNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockFieldNotFoundException" /> class.
        /// </summary>
        public BlockFieldNotFoundException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockFieldNotFoundException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        public BlockFieldNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockFieldNotFoundException" /> class.
        /// </summary>
        /// <param name="message">Text message to associate with the exception.</param>
        /// <param name="inner">Exception to set as InnerException.</param>
        public BlockFieldNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BlockFieldNotFoundException" /> class.
        /// </summary>
        /// <param name="info">Information on how to serialize an ServiceException.</param>
        /// <param name="context">Information on the streaming context for an ServiceException.</param>
        protected BlockFieldNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}