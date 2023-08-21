namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Indicates the status of a response message, so we can tell if the response
    ///     was received or just generated as a placeholder for a communication error.
    /// </summary>
    public enum Status
    {
        /// <summary>
        ///     Message response ok.
        /// </summary>
        Ok,

        /// <summary>
        ///     Indicates that there was a problem with the message data received, such as a CRC failure
        /// </summary>
        Error,

        /// <summary>
        ///     Resend previous message again.
        /// </summary>
        Retry
    }
}