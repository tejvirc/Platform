namespace Aristocrat.Monaco.G2S.Data.OptionConfig.ChangeOptionConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;

    /// <summary>
    ///     Change option config request
    /// </summary>
    public class ChangeOptionConfigRequest
    {
        /// <summary>
        ///     Gets or sets the configuration identifier.
        /// </summary>
        /// <value>
        ///     The configuration identifier.
        /// </value>
        public long ConfigurationId { get; set; }

        /// <summary>
        ///     Gets or sets the apply condition.
        /// </summary>
        /// <value>
        ///     The apply condition.
        /// </value>
        public string ApplyCondition { get; set; }

        /// <summary>
        ///     Gets or sets the disable condition.
        /// </summary>
        /// <value>
        ///     The disable condition.
        /// </value>
        public string DisableCondition { get; set; }

        /// <summary>
        ///     Gets or sets the start date time.
        /// </summary>
        /// <value>
        ///     The start date time.
        /// </value>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        ///     Gets or sets the end date time.
        /// </summary>
        /// <value>
        ///     The end date time.
        /// </value>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [restart after].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [restart after]; otherwise, <c>false</c>.
        /// </value>
        public bool RestartAfter { get; set; }

        /// <summary>
        ///     Gets or sets the options.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        public IEnumerable<Option> Options { get; set; }

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

            return Equals((ChangeOptionConfigRequest)obj);
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
                var hashCode = ConfigurationId.GetHashCode();
                hashCode = (hashCode * 397) ^ (ApplyCondition != null ? ApplyCondition.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisableCondition != null ? DisableCondition.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ StartDateTime.GetHashCode();
                hashCode = (hashCode * 397) ^ EndDateTime.GetHashCode();
                hashCode = (hashCode * 397) ^ RestartAfter.GetHashCode();
                hashCode = (hashCode * 397)
                           ^ (Options != null ? HashHelper.GetCollectionHash(Options, hashCode) : 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True if two object equal.</returns>
        protected bool Equals(ChangeOptionConfigRequest other)
        {
            return ConfigurationId == other.ConfigurationId
                   && string.Equals(ApplyCondition, other.ApplyCondition)
                   && string.Equals(DisableCondition, other.DisableCondition)
                   && StartDateTime.Equals(other.StartDateTime)
                   && EndDateTime.Equals(other.EndDateTime)
                   && RestartAfter == other.RestartAfter
                   && Options.SequenceEqual(other.Options);
        }
    }
}