namespace Aristocrat.Monaco.PackageManifest
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     The InvalidManifestException is thrown when there is an during parsing or validating a manifest file.
    /// </summary>
    [Serializable]
    public class InvalidManifestException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidManifestException" /> class.
        /// </summary>
        public InvalidManifestException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidManifestException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidManifestException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidManifestException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a null reference if no inner
        ///     exception is specified.
        /// </param>
        public InvalidManifestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidManifestException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the
        ///     exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The System.Runtime.Serialization.StreamingContext that contains contextual information about the
        ///     source or destination.
        /// </param>
        protected InvalidManifestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}