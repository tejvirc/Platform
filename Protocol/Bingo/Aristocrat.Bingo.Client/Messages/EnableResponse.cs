namespace Aristocrat.Bingo.Client.Messages
{
    public class EnableResponse : IResponse
    {
        public EnableResponse(ResponseCode code)
        {
            ResponseCode = code;
        }

        public ResponseCode ResponseCode { get; }
    }
}
