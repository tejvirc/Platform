namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(
        "ReSharper",
        "UnusedAutoPropertyAccessor.Global",
        Justification = "This gets set when created from server message and could be used by message handler in the future")]
    public class ProgressiveInfoResults : IResponse
    {
        public ProgressiveInfoResults(
            ResponseCode code,
            bool accepted,
            int gameTitleId,
            string[] progressiveLevels)
        {
            ResponseCode = code;
            Accepted = accepted;
            GameTitleId = gameTitleId;
            ProgressiveLevels = new string[progressiveLevels.Length];
            progressiveLevels.CopyTo(ProgressiveLevels, 0);
        }

        public ResponseCode ResponseCode { get; }

        public bool Accepted { get; }

        public int GameTitleId { get; }

        public string[] ProgressiveLevels { get; }
    }
}
