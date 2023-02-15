namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    public class ProgressiveClaimMessage : IMessage
    {
        public ProgressiveClaimMessage(long progressiveLevelId, long amount, int awardId)
        {
            ProgressiveLevelId = progressiveLevelId;
            Amount = amount;
            AwardId = awardId;
        }

        public long ProgressiveLevelId { get; }

        public long Amount { get; }

        public int AwardId { get; }
    }
}