namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     The progressive contribution response message
    /// </summary>
    public class ProgressiveContributionResponse : IResponse
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveContributionResponse" /> class.
        /// </summary>
        public ProgressiveContributionResponse(ResponseCode code)
        {
            ResponseCode = code;
        }

        /// <summary>
        ///     The response code
        /// </summary>
        public ResponseCode ResponseCode { get; }
    }
}
