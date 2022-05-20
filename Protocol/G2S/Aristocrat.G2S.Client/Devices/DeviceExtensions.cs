namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Extension methods for an IDevice.
    /// </summary>
    public static class DeviceExtensions
    {
        private static readonly List<string> Prefixes = new List<string> { @"G2S_", @"GTK_", @"IGT_" };

        /// <summary>
        ///     Returns a device class name without the standard G2S prefixes
        /// </summary>
        /// <param name="this">The device class.</param>
        /// <returns>A trimmed device class name</returns>
        public static string TrimmedDeviceClass(this string @this)
        {
            var prefix = Prefixes.FirstOrDefault(p => @this.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            return prefix == null ? @this : @this.Substring(prefix.Length);
        }

        /// <summary>
        ///     Returns the DeviceClass of the device prefixed for G2S communications
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <returns>A prefixed class name</returns>
        public static string PrefixedDeviceClass(this IDevice @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return $"{@this.DevicePrefix ?? Constants.DefaultPrefix}{@this.DeviceClass}";
        }

        /// <summary>
        ///     Determines if the specified host is the owner of the device.
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="hostId">The host Id.</param>
        /// <returns>true if the specified host is the owner.</returns>
        public static bool IsOwner(this IDevice @this, int hostId)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.Owner == hostId;
        }

        /// <summary>
        ///     Determines if the specified host is a guest of the device.
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="hostId">The host Id.</param>
        /// <returns>true if the specified host is a guest.</returns>
        public static bool IsGuest(this IDevice @this, int hostId)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.Guests.Any(d => d == hostId);
        }

        /// <summary>
        ///     Determines if the specified host is the configurator of the device.
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="hostId">The host Id.</param>
        /// <returns>true if the specified host is the configurator.</returns>
        public static bool IsConfigurator(this IDevice @this, int hostId)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.Configurator == hostId;
        }

        /// <summary>
        ///     Determines if the specified host is the owner or guest of the device.
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="hostId">The host Id.</param>
        /// <returns>true if the specified host is the owner or a guest.</returns>
        public static bool IsOwnerOrGuest(this IDevice @this, int hostId)
        {
            return IsOwner(@this, hostId) || IsGuest(@this, hostId);
        }

        /// <summary>
        ///     Determines if the specified host is the owner, configurator, or guest of the device.
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="hostId">The host Id.</param>
        /// <returns>true if the specified host is the owner, configurator, or a guest.</returns>
        public static bool IsMember(this IDevice @this, int hostId)
        {
            return IsOwner(@this, hostId) || IsGuest(@this, hostId) || IsConfigurator(@this, hostId);
        }

        /// <summary>
        ///     Determines if the specified class name matches the device's class
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="className">The G2S class name.</param>
        /// <returns>true if the class name is matches the device.</returns>
        public static bool IsMatching(this IDevice @this, string className)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return className == DeviceClass.G2S_all || className == @this.PrefixedDeviceClass();
        }

        /// <summary>
        ///     Determines if the specified device id matches the device's Id
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="deviceId">The device id.</param>
        /// <returns>true if the device id matches the device.</returns>
        public static bool IsMatching(this IDevice @this, int deviceId)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return deviceId == DeviceId.All || deviceId == @this.Id;
        }

        /// <summary>
        ///     Determines if the specified device id is a host oriented device
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <returns>true if the device is a host oriented device.</returns>
        public static bool IsHostOriented(this IDevice @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var baseType = @this.GetType().BaseType;

            return baseType != null &&
                   baseType.GetGenericTypeDefinition().IsAssignableFrom(typeof(HostOrientedDevice<>));
        }

        /// <summary>
        ///     Determines if a device is a single instance device.  In other words, can only one be registered.
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <returns>true if the device is a single instance device.</returns>
        public static bool IsSingle(this IDevice @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this is ISingleDevice;
        }

        /// <summary>
        ///     Determines if the device is enabled.
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <returns>true if the device is enabled.</returns>
        public static bool IsEnabled(this IDevice @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.Enabled && @this.HostEnabled;
        }

        /// <summary>
        ///     Determines if a device is equal to the specified device.
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="other">The device to compare.</param>
        /// <returns>true if the devices are equal.</returns>
        public static bool Equals(this IDevice @this, IDevice other)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (other == null)
            {
                return false;
            }

            return @this == other || @this.Id == other.Id && @this.DeviceClass == other.DeviceClass;
        }

        /// <summary>
        ///     Sets the owner for this device.
        /// </summary>
        /// <param name="this">The device.</param>
        /// <param name="host">The owner host.</param>
        /// <param name="hostConnector">The owner host connector.</param>
        public static void Owner(this IDevice @this, IHost host, IHostConnector hostConnector)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            var hostControl = hostConnector.Hosts.FirstOrDefault(h => h.Id == host.Id);
            if (hostControl != null)
            {
                if (@this is ClientDeviceBase device)
                {
                    device.HasOwner(host.Id, true);
                    device.Queue = hostControl.Queue;
                }
            }
        }

        /// <summary>
        ///     Sets the queue for this device.
        /// </summary>
        /// <param name="this">The device.</param>
        /// <param name="hostConnector">The owner host connector.</param>
        public static void SetQueue(this IDevice @this, IHostConnector hostConnector)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var hostControl = hostConnector.Hosts.FirstOrDefault(h => h.Id == @this.Owner);
            if (hostControl != null)
            {
                if (@this is ClientDeviceBase device)
                {
                    device.Queue = hostControl.Queue;
                }
            }
        }

        /// <summary>
        ///     Sets the configurator for the device.
        /// </summary>
        /// <param name="this">The device.</param>
        /// <param name="host">The configurator.</param>
        public static void Configurator(this IDevice @this, IHost host)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            var device = @this as ClientDeviceBase;

            device?.HasConfigurator(host.Id);
        }

        /// <summary>
        ///     Adds the host to the device guest list.
        /// </summary>
        /// <param name="this">The device.</param>
        /// <param name="host">The guest host.</param>
        public static void Invited(this IDevice @this, IHost host)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            var device = @this as ClientDeviceBase;

            device?.AddGuest(host.Id);
        }

        /// <summary>
        ///     Removes the host from the device guest list.
        /// </summary>
        /// <param name="this">The device.</param>
        /// <param name="host">The guest host.</param>
        public static void Uninvited(this IDevice @this, IHost host)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            var device = @this as ClientDeviceBase;

            device?.RemoveGuest(host.Id);
        }

        /// <summary>
        ///     Sets the device state
        /// </summary>
        /// <param name="this">The device.</param>
        /// <param name="active">The device state.</param>
        public static void SetStatus(this IDevice @this, bool active)
        {
            if (@this is ClientDeviceBase device)
            {
                device.Active = active;
            }
        }

        /// <summary>
        ///     Returns a G2S formatted LocaleId (ISO-639_ISO-3166).
        /// </summary>
        /// <param name="this">A device instance.</param>
        /// <param name="culture">The current <see cref="CultureInfo" />.</param>
        /// <returns>A G2S formatted LocaleId (ISO-639_ISO-3166).</returns>
        public static string LocaleId(this IDevice @this, CultureInfo culture)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var region = new RegionInfo(culture.LCID);

            // ISO-639_ISO-3166
            return $"{culture.TwoLetterISOLanguageName}_{region.TwoLetterISORegionName}";
        }
    }
}