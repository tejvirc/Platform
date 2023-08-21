namespace Aristocrat.Bingo.Client.Messages
{
    public class DisableResponse : IResponse
    {
        public DisableResponse(ResponseCode code)
        {
            ResponseCode = code;
        }

        public ResponseCode ResponseCode { get; }
    }
}
