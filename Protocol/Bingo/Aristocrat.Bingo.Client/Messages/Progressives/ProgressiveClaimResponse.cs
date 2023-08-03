namespace Aristocrat.Bingo.Client.Messages.Progressives
{
    public class ProgressiveClaimResponse : IResponse
    {
        public ProgressiveClaimResponse(ResponseCode code)
            : this(code, 0L, 0L, 0)
        {
        }

        public ProgressiveClaimResponse(
            ResponseCode code,
            long progressiveLevelId,
            long progressiveWinAmount,
            int progressiveAwardId)
        {
            ResponseCode = code;
            ProgressiveLevelId = progressiveLevelId;
            ProgressiveWinAmount = progressiveWinAmount;
            ProgressiveAwardId = progressiveAwardId;
        }

        public ResponseCode ResponseCode { get; }

        public long ProgressiveLevelId { get; }

        public long ProgressiveWinAmount { get; }

        public int ProgressiveAwardId { get; }
    }
}
