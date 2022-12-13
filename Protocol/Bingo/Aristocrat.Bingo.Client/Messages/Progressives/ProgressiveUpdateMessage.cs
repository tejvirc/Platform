namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     Progressive update message when a progressive update comes in from the server.
    /// </summary>
    /// 
    public class ProgressiveUpdateMessage : IResponse
    {
        public ProgressiveUpdateMessage(
            ResponseCode code,
            long progressiveLevel,
            long amount)
        {
            ResponseCode = code;
            ProgressiveLevel = progressiveLevel;
            Amount = amount;
        }

        public ResponseCode ResponseCode { get; set; }

        public long ProgressiveLevel { get; set; }

        public long Amount { get; set; }
    }
}
