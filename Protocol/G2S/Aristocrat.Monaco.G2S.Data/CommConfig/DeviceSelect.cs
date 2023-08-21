namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    /// <summary>
    ///     Device select model
    /// </summary>
    public class DeviceSelect
    {
        /// <summary>
        ///     Gets or sets a value indicating whether [device active].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [device active]; otherwise, <c>false</c>.
        /// </value>
        public bool DeviceActive { get; set; }

        /// <summary>
        ///     Gets or sets the device class.
        /// </summary>
        /// <value>
        ///     The device class.
        /// </value>
        public string DeviceClass { get; set; }

        /// <summary>
        ///     Gets or sets the device identifier.
        /// </summary>
        /// <value>
        ///     The device identifier.
        /// </value>
        public int DeviceId { get; set; }

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

            return Equals((DeviceSelect)obj);
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
                var hashCode = DeviceActive.GetHashCode();
                hashCode = (hashCode * 397) ^ (DeviceClass != null ? DeviceClass.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ DeviceId;
                return hashCode;
            }
        }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True if two object equal</returns>
        protected bool Equals(DeviceSelect other)
        {
            return DeviceActive == other.DeviceActive
                   && string.Equals(DeviceClass, other.DeviceClass)
                   && DeviceId == other.DeviceId;
        }
    }
}