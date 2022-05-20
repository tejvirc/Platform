namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Thrown when an error occurs in the localization service.
    /// </summary>
    [Serializable]
    public class LocalizationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationException" /> class.
        /// </summary>
        public LocalizationException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        public LocalizationException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationException" /> class.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public LocalizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationException" /> class.
        /// </summary>
        /// <param name="serializationInfo">The SerializationInfo</param>
        /// <param name="streamingContext">The StreamingContext</param>
        protected LocalizationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}