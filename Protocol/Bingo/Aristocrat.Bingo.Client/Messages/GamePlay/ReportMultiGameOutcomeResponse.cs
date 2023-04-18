namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    /// <summary>
    ///     Provides the response to a multi-game outcome
    /// </summary>
    public class ReportMultiGameOutcomeResponse
    {
        public ReportMultiGameOutcomeResponse(ResponseCode responseCode)
        {
            ResponseCode = responseCode;
        }

        /// <summary>Gets the response code</summary>
        public ResponseCode ResponseCode { get; }

    }
}