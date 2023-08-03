namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    public class ProgressiveAwardResponse : IResponse
    {
        public ProgressiveAwardResponse(ResponseCode responseCode)
        {
            ResponseCode = responseCode;
        }

        public ResponseCode ResponseCode { get; }
    }
}
