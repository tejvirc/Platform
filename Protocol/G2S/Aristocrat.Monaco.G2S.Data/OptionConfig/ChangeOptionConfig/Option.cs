namespace Aristocrat.Monaco.G2S.Data.OptionConfig.ChangeOptionConfig
{
    using System.Linq;

    /// <summary>
    ///     Config option
    /// </summary>
    public class Option
    {
        /// <summary>
        ///     Gets or sets the device identifier.
        /// </summary>
        /// <value>
        ///     The device identifier.
        /// </value>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets the device class.
        /// </summary>
        /// <value>
        ///     The device class.
        /// </value>
        public string DeviceClass { get; set; }

        /// <summary>
        ///     Gets or sets the option group identifier.
        /// </summary>
        /// <value>
        ///     The option group identifier.
        /// </value>
        public string OptionGroupId { get; set; }

        /// <summary>
        ///     Gets or sets the option identifier.
        /// </summary>
        /// <value>
        ///     The option identifier.
        /// </value>
        public string OptionId { get; set; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        public OptionCurrentValue[] OptionValues { get; set; }

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

            return Equals((Option)obj);
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
                var valuesCode = 0;
                if (OptionValues != null)
                {
                    foreach (var val in OptionValues)
                    {
                        valuesCode ^= val.Value.GetHashCode();
                    }
                }

                var hashCode = DeviceId;
                hashCode = (hashCode * 397) ^ (DeviceClass != null ? DeviceClass.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OptionGroupId != null ? OptionGroupId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OptionId != null ? OptionId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ valuesCode;
                return hashCode;
            }
        }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True if two object equal.</returns>
        protected bool Equals(Option other)
        {
            return DeviceId == other.DeviceId
                   && string.Equals(DeviceClass, other.DeviceClass)
                   && string.Equals(OptionGroupId, other.OptionGroupId)
                   && string.Equals(OptionId, other.OptionId)
                   && OptionValues.SequenceEqual(other.OptionValues);
        }
    }
}