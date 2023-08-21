namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Represents record from CommHostConfigDevice data table.
    /// </summary>
    public class CommHostConfigDevice : BaseEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigDevice" /> class.
        /// </summary>
        public CommHostConfigDevice()
            : this(0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommHostConfigDevice" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public CommHostConfigDevice(long id)
        {
            Id = id;
        }

        /// <summary>
        ///     Gets or sets current host type.
        /// </summary>
        public CommHostConfigType DeviceType { get; set; }

        /// <summary>
        ///     Gets or sets device type.
        /// </summary>
        public DeviceClass DeviceClass { get; set; }

        /// <summary>
        ///     Gets or sets device id.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether device is active or not.
        /// </summary>
        public bool IsDeviceActive { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the deviceActive attribute can be modified by the commConfig host to make
        ///     the device active or inactive..
        /// </summary>
        public bool CanModActiveRemote { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the device can be re-assigned to another host by the commConfig host.This
        ///     is set to false if the
        ///     device can only be EGM-owned.
        /// </summary>
        public bool CanModOwnerRemote { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether remote config is available or not.
        /// </summary>
        public bool CanModConfigRemote { get; set; }

        /// <summary>
        ///     Gets or sets the comm host configuration item identifier.
        /// </summary>
        /// <value>
        ///     The comm host configuration item identifier.
        /// </value>
        public long CommHostConfigItemId { get; set; }

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

            return Equals((CommHostConfigDevice)obj);
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
                var hashCode = (int)DeviceType;
                hashCode = (hashCode * 397) ^ (int)DeviceClass;
                hashCode = (hashCode * 397) ^ DeviceId;
                hashCode = (hashCode * 397) ^ IsDeviceActive.GetHashCode();
                hashCode = (hashCode * 397) ^ CanModActiveRemote.GetHashCode();
                hashCode = (hashCode * 397) ^ CanModOwnerRemote.GetHashCode();
                hashCode = (hashCode * 397) ^ CanModConfigRemote.GetHashCode();
                hashCode = (hashCode * 397) ^ CommHostConfigItemId.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        ///     Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True if two object equal.</returns>
        protected bool Equals(CommHostConfigDevice other)
        {
            return DeviceType == other.DeviceType
                   && DeviceClass == other.DeviceClass
                   && DeviceId == other.DeviceId
                   && IsDeviceActive == other.IsDeviceActive
                   && CanModActiveRemote == other.CanModActiveRemote
                   && CanModOwnerRemote == other.CanModOwnerRemote
                   && CanModConfigRemote == other.CanModConfigRemote
                   && CommHostConfigItemId == other.CommHostConfigItemId;
        }
    }
}