namespace Aristocrat.Monaco.Bingo.Services.Configuration
{
    using System.Collections.Generic;
    using System.Linq;

    public sealed class GameDetailsRequiresResetComparer : IEqualityComparer<ServerGameConfiguration>
    {
        private static readonly BetRtpEqualityComparer BetDetailsComparer = new();

        public bool Equals(ServerGameConfiguration x, ServerGameConfiguration y)
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

            return x.GameTitleId == y.GameTitleId &&
                   x.ThemeSkinId == y.ThemeSkinId &&
                   x.PaytableId == y.PaytableId &&
                   x.Denomination == y.Denomination &&
                   x.BetInformationDetails.SequenceEqual(y.BetInformationDetails, BetDetailsComparer) &&
                   x.EvaluationTypePaytable == y.EvaluationTypePaytable &&
                   x.CrossGameProgressiveEnabled == y.CrossGameProgressiveEnabled;
        }

        public int GetHashCode(ServerGameConfiguration obj)
        {
            unchecked
            {
                var hashCode = obj.GameTitleId.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.ThemeSkinId.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.PaytableId.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Denomination.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.BetInformationDetails != null ? obj.BetInformationDetails.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)obj.EvaluationTypePaytable;
                hashCode = (hashCode * 397) ^ obj.CrossGameProgressiveEnabled.GetHashCode();
                return hashCode;
            }
        }
    }
}