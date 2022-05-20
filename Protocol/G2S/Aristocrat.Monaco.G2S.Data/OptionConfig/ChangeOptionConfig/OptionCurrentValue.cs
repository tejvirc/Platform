namespace Aristocrat.Monaco.G2S.Data.OptionConfig.ChangeOptionConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using Model;

    /// <summary>
    ///     Option current value
    /// </summary>
    public class OptionCurrentValue
    {
        /// <summary>
        ///     Gets or sets the parameter identifier.
        /// </summary>
        /// <value>
        ///     The parameter identifier.
        /// </value>
        public string ParamId { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        ///     Gets or sets the type of the parameter.
        /// </summary>
        /// <value>
        ///     The type of the parameter.
        /// </value>
        public OptionConfigParameterType ParameterType { get; set; }

        /// <summary>
        ///     Gets or sets the child values.
        /// </summary>
        /// <value>
        ///     The child values.
        /// </value>
        public IEnumerable<OptionCurrentValue> ChildValues { get; set; }

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

            return Equals((OptionCurrentValue)obj);
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
                var hashCode = ParamId?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Value.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)ParameterType;
                hashCode = (hashCode * 397)
                           ^ (ChildValues != null ? HashHelper.GetCollectionHash(ChildValues, hashCode) : 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True if two object equal.</returns>
        protected bool Equals(OptionCurrentValue other)
        {
            return string.Equals(ParamId, other.ParamId)
                   && Value == other.Value
                   && ParameterType == other.ParameterType
                   && CompareChildValues(other.ChildValues);
        }

        /// <summary>
        ///     Compares the child values.
        /// </summary>
        /// <param name="otherChildValues">The other child values.</param>
        /// <returns>Returns true if two list are the same.</returns>
        private bool CompareChildValues(IEnumerable<OptionCurrentValue> otherChildValues)
        {
            if (ChildValues != null && otherChildValues == null)
            {
                return false;
            }

            if (ChildValues == null && otherChildValues != null)
            {
                return false;
            }

            if (ChildValues == null && otherChildValues == null)
            {
                return true;
            }

            if (ChildValues != null && otherChildValues != null)
            {
                return ChildValues.SequenceEqual(otherChildValues);
            }

            return false;
        }
    }
}