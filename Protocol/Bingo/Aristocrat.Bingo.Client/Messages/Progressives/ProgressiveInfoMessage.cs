namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    using System.Collections.Generic;

    public class ProgressiveInfoMessage : IResponse
    {
        public ProgressiveInfoMessage(
            ResponseCode code,
            bool accepted,
            int gameTitleId,
            string authenticationToken,
            IEnumerable<ProgressiveLevelInfo> progressiveLevels,
            IEnumerable<int> metersToReport)
        {
            ResponseCode = code;
            Accepted = accepted;
            GameTitleId = gameTitleId;
            AuthenticationToken = authenticationToken;
            ProgressiveLevels = new List<ProgressiveLevelInfo>(progressiveLevels);
            MetersToReport = new List<int>(metersToReport);
        }

        public ResponseCode ResponseCode { get; }

        public bool Accepted { get; }

        public int GameTitleId { get; }

        public string AuthenticationToken { get; }

        public IReadOnlyCollection<ProgressiveLevelInfo> ProgressiveLevels { get; }

        public IReadOnlyCollection<int> MetersToReport { get; }

    }
}
