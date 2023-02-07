namespace Aristocrat.Bingo.Client.Messages
{
    public class ReportEventResponse : IResponse
    {
        public ReportEventResponse(ResponseCode responseCode, long eventId)
        {
            ResponseCode = responseCode;
            EventId = eventId;
        }

        public ResponseCode ResponseCode { get; }

        public long EventId { get; }
    }
}