namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     The response to a ProgressiveUpdateRequestMessage sent to the server.
    /// </summary>
    public class ProgressiveUpdateResponse : IResponse
    {
        public ProgressiveUpdateResponse(ResponseCode code)
        {
            ResponseCode = code;
        }

        public ResponseCode ResponseCode { get; }
    }
}
