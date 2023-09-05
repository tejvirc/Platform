namespace Aristocrat.Monaco.PackageManifest.Gsa
{
    using System;

    /// <summary>
    ///     Return to Player value exception represents an error related to RTP data
    /// </summary>
    public class RtpValueException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpValueException" /> class.
        /// </summary>
        public RtpValueException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpValueException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public RtpValueException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RtpValueException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RtpValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}