namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    ///     Progressive update message.
    /// </summary>
    /// 
    public class ProgressiveUpdateMessage : IMessage
    {
        public ProgressiveUpdateMessage(
            long progressiveLevel,
            long amount)
        {
            ProgressiveLevel = progressiveLevel;
            Amount = amount;
        }

        public long ProgressiveLevel { get; set; }

        public long Amount { get; set; }
    }
}
