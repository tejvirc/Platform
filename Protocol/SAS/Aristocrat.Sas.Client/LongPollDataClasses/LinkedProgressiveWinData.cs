namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public class LinkedProgressiveWinData
    {
        public LinkedProgressiveWinData(int levelId, long amount, string levelName)
        {
            LevelId = levelId;
            Amount = amount;
            LevelName = levelName;
        }

        public int LevelId { get; }

        public long Amount { get; }

        public string LevelName { get; }
    }
}