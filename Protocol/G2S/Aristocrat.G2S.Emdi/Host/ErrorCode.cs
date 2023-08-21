namespace Aristocrat.G2S.Emdi.Host
{
    /// <summary>
    /// EMDI error codes to return to media player content
    /// </summary>
    public enum EmdiErrorCode
    {
        /// <summary>
        /// Error not set
        /// </summary>
        UnknownError = -1,

        /// <summary>
        /// No errors
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Command not allowed.
        /// </summary>
        NotAllowed = 1,

        /// <summary>
        /// Invalid XML.
        /// </summary>
        InvalidXml = 2,

        /// <summary>
        /// Communications session not established.
        /// </summary>
        NoSession = 3,

        /// <summary>
        /// Request already pending.
        /// </summary>
        AlreadyPending = 4,

        /// <summary>
        /// SessionId out of order.
        /// </summary>
        SessionOrder = 5,

        /// <summary>
        /// Message timeout
        /// </summary>
        SessionExpired = 6,

        /// <summary>
        /// Invalid Event Code.
        /// </summary>
        InvalidEventCode = 201,

        /// <summary>
        /// Invalid Meter Name.
        /// </summary>
        InvalidMeterName = 301,

        /// <summary>
        /// EGM Local Media Display Interface Not Open.
        /// </summary>
        InterfaceNotOpen = 402,

        /// <summary>
        /// Content-to-Content Communications Not Permitted.
        /// </summary>
        ContentToContentNotPermitted = 403,
    }
}
