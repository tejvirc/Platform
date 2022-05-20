namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Contracts;

    public class GameSummary
    {
        private int _oneHundredPercent = 100;

        public GameSummary(string name, decimal rtp)
        {
            Name = name;
            BlendedRTP = rtp.GetRtpString();
            BlendedHold = (_oneHundredPercent - rtp).GetRtpString();
        }

        public string Name { get; }

        public string BlendedRTP { get; }

        public string BlendedHold { get; }
    }
}