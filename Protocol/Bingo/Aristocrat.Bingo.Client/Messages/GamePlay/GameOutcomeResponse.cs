namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    public class GameOutcomeResponse : IResponse
    {
        public GameOutcomeResponse(ResponseCode code)
        {
            ResponseCode = code;
        }

        public ResponseCode ResponseCode { get; }
    }
}
