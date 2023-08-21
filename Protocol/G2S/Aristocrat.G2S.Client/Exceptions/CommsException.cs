namespace Aristocrat.G2S.Client.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     CommsException
    /// </summary>
    [Serializable]
    public class CommsException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommsException" /> class.
        /// </summary>
        public CommsException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommsException" /> class.
        /// </summary>
        /// <param name="message">the message</param>
        public CommsException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommsException" /> class.
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="innerException">the innerException</param>
        public CommsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommsException" /> class.
        /// </summary>
        /// <param name="info">the info</param>
        /// <param name="context">the context</param>
        protected CommsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}