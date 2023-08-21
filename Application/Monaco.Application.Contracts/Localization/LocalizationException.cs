namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Throw when there is a localization error.
    /// </summary>
    [Serializable]
    public class LocalizationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationException" /> class.
        /// </summary>
        public LocalizationException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationException" /> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public LocalizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalizationException" /> class.
        /// </summary>
        /// <param name="serializationInfo">The <see cref="SerializationInfo"/> object.</param>
        /// <param name="streamingContext">The <see cref="StreamingContext"/>.</param>
        protected LocalizationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
