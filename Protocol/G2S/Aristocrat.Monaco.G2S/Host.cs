namespace Aristocrat.Monaco.G2S
{
    using System;
    using Aristocrat.G2S.Client;

    /// <summary>
    ///     IHost model implementation
    /// </summary>
    /// <seealso cref="IHost" />
    internal class Host : IHost
    {
        /// <inheritdoc />
        public int Index { get; internal set; }

        /// <inheritdoc />
        public int Id { get; internal set; }

        /// <inheritdoc />
        public Uri Address { get; internal set; }

        /// <inheritdoc />
        public bool Registered { get; internal set; }

        /// <inheritdoc />
        public bool RequiredForPlay { get; internal set; }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Host)obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Index;
                hashCode = (hashCode * 397) ^ Id;
                hashCode = (hashCode * 397) ^ (Address != null ? Address.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Registered.GetHashCode();
                hashCode = (hashCode * 397) ^ RequiredForPlay.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True if two object equal.</returns>
        protected bool Equals(Host other)
        {
            return other != null &&
                   Index == other.Index
                   && Id == other.Id
                   && Equals(Address, other.Address)
                   && Registered == other.Registered
                   && RequiredForPlay == other.RequiredForPlay;
        }
    }
}