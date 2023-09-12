namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Contracts.Rtp;

    public class GameSummary
    {
        private const decimal OneHundredPercent = 100m;

        public GameSummary(string name, decimal rtp)
        {
            Name = name;
            BlendedRTP = rtp.ToRtpString();
            BlendedHold = (OneHundredPercent - rtp).ToRtpString();
        }

        public string Name { get; }

        public string BlendedRTP { get; }

        public string BlendedHold { get; }
    }
}