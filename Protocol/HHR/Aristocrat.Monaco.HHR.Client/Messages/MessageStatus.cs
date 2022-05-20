namespace Aristocrat.Monaco.Hhr.Client.Messages
{
    /// <summary>
    ///     Indicates the status of a response message, so we can tell if the response
    ///     was received or just generated as a placeholder for a communication error.
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>
        ///     Success
        /// </summary>
        Success,

        /// <summary>
        ///     A cancelled message
        /// </summary>
        Cancelled,

        /// <summary>
        ///     Message timed out.
        /// </summary>
        TimedOut,

        /// <summary>
        ///     Communication with central server disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        ///     Unable to send to central server.
        /// </summary>
        UnableToSend,

        /// <summary>
        ///     Error while sending through pipeline
        /// </summary>
        PipelineError,

        /// <summary>
        ///     Response is un-expected.
        /// </summary>
        UnexpectedResponse,

        /// <summary>
        ///     No response is expected.
        /// </summary>
        NoResponse,
        /// <summary>
        ///     Some other error.
        /// </summary>
        OtherError
    }
}