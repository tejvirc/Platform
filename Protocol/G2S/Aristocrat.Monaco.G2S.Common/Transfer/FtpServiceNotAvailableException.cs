namespace Aristocrat.Monaco.G2S.Common.Transfer
{
    using System;

    /// <summary>
    ///     FTP Service Not Available exception
    /// </summary>
    [Serializable]
    public class FtpServiceNotAvailableException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FtpServiceNotAvailableException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FtpServiceNotAvailableException(string message)
            : base(message)
        {
        }
    }
}