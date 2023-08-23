namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Contracts;
    using Contracts.Progressives;
    using MVVM.Model;

    public class PaytableDisplay : BaseNotify
    {
        public PaytableDisplay(IGameDetail gameDetail, long denom, bool displayRtpAsRange)
        {
            GameDetail = gameDetail;
            Rtp = gameDetail.GetTotalJurisdictionRtpRange(denom).Rtp;
            DisplayText = $"{(displayRtpAsRange ? Rtp.GetRtpString() : Rtp.Minimum.GetRtpString())} v{gameDetail.VariationId}";
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

        public void UpdateDisplayText() => RaisePropertyChanged(nameof(DisplayText));

        protected bool Equals(PaytableDisplay other)
        {
            return GameDetail.Id == other.GameDetail.Id;
        }
    }
}