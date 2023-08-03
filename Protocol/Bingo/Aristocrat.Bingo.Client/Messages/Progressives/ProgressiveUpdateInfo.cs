namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    /// <summary>
    /// Progressive update information
    /// </summary>
    public class ProgressiveUpdateInfo
    {
        public ProgressiveUpdateInfo(long newValue, long progressiveLevel)
        {
            NewValue = newValue;
            ProgressiveLevel = progressiveLevel;
        }

        public long NewValue { get; }

        public long ProgressiveLevel { get; }
    }
}
