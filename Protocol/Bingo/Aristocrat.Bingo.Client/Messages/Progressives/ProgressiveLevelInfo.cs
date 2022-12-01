namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    public class ProgressiveLevelInfo
    {
        public ProgressiveLevelInfo(
            long progressiveLevel,
            int sequenceNumber)
        {
            ProgressiveLevel = progressiveLevel;
            SequenceNumber = sequenceNumber;
        }

        public long ProgressiveLevel { get; }

        public int SequenceNumber { get; }
    }
}
