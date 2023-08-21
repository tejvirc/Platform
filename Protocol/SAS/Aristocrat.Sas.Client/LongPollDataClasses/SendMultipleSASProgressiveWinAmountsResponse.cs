using System.Collections.Generic;

namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    public class SendMultipleSasProgressiveWinAmountsResponse : LongPollResponse
    {
        public SendMultipleSasProgressiveWinAmountsResponse(IEnumerable<LinkedProgressiveWinData> progressivesWon, int groupId)
        {
            GroupId = groupId;
            ProgressivesWon = new List<LinkedProgressiveWinData>(progressivesWon);
        }

        public int GroupId { get; }

        public IReadOnlyCollection<LinkedProgressiveWinData> ProgressivesWon { get; }
    }
}