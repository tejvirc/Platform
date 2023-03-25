namespace Aristocrat.Bingo.Client.Messages
{
    /// <summary>
    ///     The response message for an event
    /// </summary>
    public class ReportEventResponse : IResponse
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReportEventResponse"/>
        /// </summary>
        /// <param name="responseCode">The response code that was received</param>
        /// <param name="eventId">The event ID this response is for</param>
        public ReportEventResponse(ResponseCode responseCode, long eventId)
        {
            ResponseCode = responseCode;
            EventId = eventId;
        }

        /// <summary>
        ///     Gets the response code
        /// </summary>
        public ResponseCode ResponseCode { get; }

        /// <summary>
        ///     Gets the event ID
        /// </summary>
        public long EventId { get; }
    }
}