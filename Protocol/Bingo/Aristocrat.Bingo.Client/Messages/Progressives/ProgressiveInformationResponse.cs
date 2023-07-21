namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    public class ProgressiveInformationResponse : IResponse
    {
        public ProgressiveInformationResponse(ResponseCode code)
        {
            ResponseCode = code;
        }

        public ResponseCode ResponseCode { get; }
    }
}
