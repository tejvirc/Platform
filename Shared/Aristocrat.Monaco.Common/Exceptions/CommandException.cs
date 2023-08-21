namespace Aristocrat.Monaco.Common.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Command exception.
    /// </summary>
    [Serializable]
    public class CommandException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandException" /> class.
        /// </summary>
        public CommandException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandException" /> class.
        /// </summary>
        /// <param name="message">the message</param>
        public CommandException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandException" /> class.
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="innerException">the innerException</param>
        public CommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandException" /> class.
        /// </summary>
        /// <param name="info">the info</param>
        /// <param name="context">the context</param>
        protected CommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}