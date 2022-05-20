namespace Aristocrat.Monaco.G2S.Exceptions
{
    using System;

    /// <summary>
    ///     Invalid certificate configuration exception
    /// </summary>
    [Serializable]
    public class InvalidCertificateConfigurationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidCertificateConfigurationException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidCertificateConfigurationException(string message)
            : base(message)
        {
        }
    }
}