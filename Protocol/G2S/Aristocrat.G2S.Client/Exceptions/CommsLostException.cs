namespace Aristocrat.G2S.Client.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     CommsLostException
    /// </summary>
    [Serializable]
    public class CommsLostException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommsLostException" /> class.
        /// </summary>
        public CommsLostException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommsLostException" /> class.
        /// </summary>
        /// <param name="message">message</param>
        public CommsLostException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommsLostException" /> class.
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">innerException</param>
        public CommsLostException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommsLostException" /> class.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="context">context</param>
        protected CommsLostException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}