namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System.Collections.Generic;

    public sealed class BetRtpEqualityComparer : IEqualityComparer<ServerBetInformationDetail>
    {
        public bool Equals(ServerBetInformationDetail x, ServerBetInformationDetail y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Bet == y.Bet && x.Rtp == y.Rtp;
        }

        public int GetHashCode(ServerBetInformationDetail obj)
        {
            unchecked
            {
                return (obj.Bet.GetHashCode() * 397) ^ obj.Rtp.GetHashCode();
            }
        }
    }
}