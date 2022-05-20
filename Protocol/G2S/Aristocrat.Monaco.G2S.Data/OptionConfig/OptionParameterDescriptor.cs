namespace Aristocrat.Monaco.G2S.Data.OptionConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using Model;

    /// <summary>
    ///     option parameter descriptor
    /// </summary>
    public class OptionParameterDescriptor
    {
        /// <summary>
        ///     Gets or sets the parameter identifier.
        /// </summary>
        /// <value>
        ///     The parameter identifier.
        /// </value>
        public string ParameterId { get; set; }

        /// <summary>
        ///     Gets or sets the type of the parameter.
        /// </summary>
        /// <value>
        ///     The type of the parameter.
        /// </value>
        public OptionConfigParameterType ParameterType { get; set; }

        /// <summary>
        ///     Gets or sets the child items.
        /// </summary>
        /// <value>
        ///     The child items.
        /// </value>
        public IEnumerable<OptionParameterDescriptor> ChildItems { get; set; }

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

            return Equals((OptionParameterDescriptor)obj);
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
                var hashCode = ParameterId != null ? ParameterId.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (int)ParameterType;
                hashCode = (hashCode * 397)
                           ^ (ChildItems != null ? HashHelper.GetCollectionHash(ChildItems, hashCode) : 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>>True if two object is equal.</returns>
        protected bool Equals(OptionParameterDescriptor other)
        {
            return string.Equals(
                       ParameterId,
                       other.ParameterId)
                   && ParameterType == other.ParameterType
                   && CompareChildItems(other.ChildItems);
        }

        /// <summary>
        ///     Compares the child items.
        /// </summary>
        /// <param name="otherChildItems">The other child items.</param>
        /// <returns>Returns true if two list are the same.</returns>
        private bool CompareChildItems(IEnumerable<OptionParameterDescriptor> otherChildItems)
        {
            if (ChildItems != null && otherChildItems == null)
            {
                return false;
            }

            if (ChildItems == null && otherChildItems != null)
            {
                return false;
            }

            if (ChildItems == null && otherChildItems == null)
            {
                return true;
            }

            if (ChildItems != null && otherChildItems != null)
            {
                return ChildItems.SequenceEqual(otherChildItems);
            }

            return false;
        }
    }
}