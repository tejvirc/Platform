namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    using System.Collections.Generic;

    public class ProgressiveInfoResults : IResponse
    {
        public ProgressiveInfoResults(
            ResponseCode code,
            bool accepted,
            int gameTitleId,
            IEnumerable<ProgressiveLevelInfo> progressiveLevels)
        {
            ResponseCode = code;
            Accepted = accepted;
            GameTitleId = gameTitleId;
            ProgressiveLevels = new List<ProgressiveLevelInfo>(progressiveLevels);
        }

        public ResponseCode ResponseCode { get; }

        public bool Accepted { get; }

        public int GameTitleId { get; }

        public IReadOnlyCollection<ProgressiveLevelInfo> ProgressiveLevels { get; }
    }
}
