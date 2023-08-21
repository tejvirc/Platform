namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     The equality comparer for <see cref="PortAssignment"/>
    /// </summary>
    public sealed class PortAssignmentEqualityComparer : IEqualityComparer<PortAssignment>
    {
        /// <inheritdoc />
        public bool Equals(PortAssignment x, PortAssignment y)
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

            return x.IsDualHost == y.IsDualHost && x.AftPort == y.AftPort &&
                   x.GeneralControlPort == y.GeneralControlPort && x.LegacyBonusPort == y.LegacyBonusPort &&
                   x.ProgressivePort == y.ProgressivePort && x.ValidationPort == y.ValidationPort &&
                   x.GameStartEndHosts == y.GameStartEndHosts && x.Host1NonSasProgressiveHitReporting == y.Host1NonSasProgressiveHitReporting &&
                   x.Host2NonSasProgressiveHitReporting == y.Host2NonSasProgressiveHitReporting;
        }

        /// <inheritdoc />
        public int GetHashCode(PortAssignment obj)
        {
            unchecked
            {
                var hashCode = obj.IsDualHost.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)obj.AftPort;
                hashCode = (hashCode * 397) ^ (int)obj.GeneralControlPort;
                hashCode = (hashCode * 397) ^ (int)obj.LegacyBonusPort;
                hashCode = (hashCode * 397) ^ (int)obj.ProgressivePort;
                hashCode = (hashCode * 397) ^ (int)obj.ValidationPort;
                hashCode = (hashCode * 397) ^ (int)obj.GameStartEndHosts;
                return hashCode;
            }
        }
    }
}