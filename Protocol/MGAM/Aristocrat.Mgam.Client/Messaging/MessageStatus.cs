namespace Aristocrat.Mgam.Client.Messaging
{
    /// <summary>
    ///     Defines codes for communication exceptions.
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>Indicates that the message was sent successfully.</summary>
        Success,

        /// <summary>Timed out waiting for response.</summary>
        TimedOut,

        /// <summary>Connection was closed while waiting for response.</summary>
        CommsLost,

        /// <summary>Indicates an error receiving the response.</summary>
        ResponseError,

        /// <summary>Indicates an error sending a request.</summary>
        SendError,

        /// <summary>Indicates the caller canceled the operation.</summary>
        Aborted,

        /// <summary>Indicates that an unknown error occurred.</summary>
        UnknownError
    }
}
