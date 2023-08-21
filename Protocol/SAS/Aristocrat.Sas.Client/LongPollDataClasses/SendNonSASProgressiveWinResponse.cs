namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    public class SendNonSASProgressiveWinResponse : LongPollResponse
    {
        public SendNonSASProgressiveWinResponse(IEnumerable<NonSasProgressiveWinData> progressivesWon)
        {
            ProgressivesWon = new List<NonSasProgressiveWinData>(progressivesWon);
            NumberOfLevels = (byte)ProgressivesWon.Count;
        }

        public byte NumberOfLevels { get; }

        public IReadOnlyCollection<NonSasProgressiveWinData> ProgressivesWon { get; }
    }
}