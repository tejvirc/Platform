namespace Aristocrat.Monaco.Sas.Storage.Models
{
    using System.Collections.Generic;

    /// <summary>
    ///     An equality comparer for <see cref="Host"/>
    /// </summary>
    public sealed class HostEqualityComparer : IEqualityComparer<Host>
    {
        /// <inheritdoc />
        public bool Equals(Host x, Host y)
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

            return x.ComPort == y.ComPort && x.SasAddress == y.SasAddress && x.AccountingDenom == y.AccountingDenom;
        }

        /// <inheritdoc />
        public int GetHashCode(Host obj)
        {
            unchecked
            {
                var hashCode = obj.ComPort;
                hashCode = (hashCode * 397) ^ obj.SasAddress.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.AccountingDenom.GetHashCode();
                return hashCode;
            }
        }
    }
}