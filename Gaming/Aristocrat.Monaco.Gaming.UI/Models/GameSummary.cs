﻿namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Contracts;

    public class GameSummary
    {
        private const decimal OneHundredPercent = 100m;

        public GameSummary(string name, decimal rtp)
        {
            Name = name;
            BlendedRTP = rtp.GetRtpString();
            BlendedHold = (OneHundredPercent - rtp).GetRtpString();
        }

        public string Name { get; }

        public string BlendedRTP { get; }

        public string BlendedHold { get; }
    }
}