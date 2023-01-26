namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Contracts;
    using Contracts.Rtp;

    public class PaytableDisplay
    {
        public PaytableDisplay(IGameDetail gameDetail, long denom, bool displayRtpAsRange)
        {
            GameDetail = gameDetail;
            Rtp = gameDetail.GetTotalJurisdictionRtpRange(denom).Rtp;
            DisplayText = $"{(displayRtpAsRange ? Rtp.ToString() : Rtp.Minimum.GetRtpString())} v{gameDetail.VariationId}";
        }

        public IGameDetail GameDetail { get; }

        public RtpRange Rtp { get; }

        public override bool Equals(object obj)
        {
            return obj != null &&
                   (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((PaytableDisplay)obj));
        }

        public override int GetHashCode()
        {
            return GameDetail?.Id.GetHashCode() ?? 0;
        }

        public string DisplayText { get; }

        protected bool Equals(PaytableDisplay other)
        {
            return GameDetail.Id == other.GameDetail.Id;
        }
    }
}