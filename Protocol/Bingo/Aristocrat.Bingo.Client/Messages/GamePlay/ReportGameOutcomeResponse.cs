namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    public class ReportGameOutcomeResponse : IResponse
    {
        public ReportGameOutcomeResponse(ResponseCode responseCode)
        {
            ResponseCode = responseCode;
        }

        public ResponseCode ResponseCode { get; }
    }
}