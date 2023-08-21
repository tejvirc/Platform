namespace Aristocrat.Monaco.G2S.Data.CommConfig
{
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;

    /// <summary>
    ///     Set host item model
    /// </summary>
    public class SetHostItem
    {
        /// <summary>
        ///     Gets or sets the index of the host.
        /// </summary>
        /// <value>
        ///     The index of the host.
        /// </value>
        public int HostIndex { get; set; }

        /// <summary>
        ///     Gets or sets the host identifier.
        /// </summary>
        /// <value>
        ///     The host identifier.
        /// </value>
        public int HostId { get; set; }

        /// <summary>
        ///     Gets or sets the host location.
        /// </summary>
        /// <value>
        ///     The host location.
        /// </value>
        public string HostLocation { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [host registered].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [host registered]; otherwise, <c>false</c>.
        /// </value>
        public bool HostRegistered { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [use default configuration].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [use default configuration]; otherwise, <c>false</c>.
        /// </value>
        public bool UseDefaultConfig { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [required for play].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [required for play]; otherwise, <c>false</c>.
        /// </value>
        public bool RequiredForPlay { get; set; }

        /// <summary>
        ///     Gets or sets the time to live.
        /// </summary>
        /// <value>
        ///     The time to live.
        /// </value>
        public int TimeToLive { get; set; }

        /// <summary>
        ///     Gets or sets the no response timer.
        /// </summary>
        /// <value>
        ///     The no response timer.
        /// </value>
        public int NoResponseTimer { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [allow multicast].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [allow multicast]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMulticast { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [display comm fault].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [display comm fault]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayCommFault { get; set; }

        /// <summary>
        ///     Gets or sets the owned devices.
        /// </summary>
        /// <value>
        ///     The owned devices.
        /// </value>
        public IEnumerable<DeviceSelect> OwnedDevices { get; set; }

        /// <summary>
        ///     Gets or sets the configuration device.
        /// </summary>
        /// <value>
        ///     The configuration device.
        /// </value>
        public IEnumerable<DeviceSelect> ConfigDevices { get; set; }

        /// <summary>
        ///     Gets or sets the guest device.
        /// </summary>
        /// <value>
        ///     The guest device.
        /// </value>
        public IEnumerable<DeviceSelect> GuestDevices { get; set; }

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

            return Equals((SetHostItem)obj);
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
                var hashCode = HostIndex;
                hashCode = (hashCode * 397) ^ HostId;
                hashCode = (hashCode * 397) ^ (HostLocation != null ? HostLocation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ HostRegistered.GetHashCode();
                hashCode = (hashCode * 397) ^ UseDefaultConfig.GetHashCode();
                hashCode = (hashCode * 397) ^ RequiredForPlay.GetHashCode();
                hashCode = (hashCode * 397) ^ TimeToLive;
                hashCode = (hashCode * 397) ^ NoResponseTimer;
                hashCode = (hashCode * 397) ^ AllowMulticast.GetHashCode();
                hashCode = (hashCode * 397) ^ DisplayCommFault.GetHashCode();
                hashCode = (hashCode * 397) ^
                           (OwnedDevices != null ? HashHelper.GetCollectionHash(OwnedDevices, hashCode) : 0);
                hashCode = (hashCode * 397) ^
                           (ConfigDevices != null ? HashHelper.GetCollectionHash(ConfigDevices, hashCode) : 0);
                hashCode = (hashCode * 397) ^
                           (GuestDevices != null ? HashHelper.GetCollectionHash(GuestDevices, hashCode) : 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Equals the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True if two object equal.</returns>
        protected bool Equals(SetHostItem other)
        {
            return HostIndex == other.HostIndex
                   && HostId == other.HostId
                   && string.Equals(HostLocation, other.HostLocation)
                   && HostRegistered == other.HostRegistered
                   && UseDefaultConfig == other.UseDefaultConfig
                   && RequiredForPlay == other.RequiredForPlay
                   && TimeToLive == other.TimeToLive
                   && NoResponseTimer == other.NoResponseTimer
                   && AllowMulticast == other.AllowMulticast
                   && DisplayCommFault == other.DisplayCommFault
                   && OwnedDevices.SequenceEqual(other.OwnedDevices)
                   && GuestDevices.SequenceEqual(other.GuestDevices)
                   && ConfigDevices.SequenceEqual(other.ConfigDevices);
        }
    }
}