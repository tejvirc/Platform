namespace Aristocrat.Monaco.Hardware.Contracts.Cabinet
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Kernel;

    /// <summary>
    ///     A base device event.
    /// </summary>
    public class BaseDeviceEvent : BaseEvent
    {
        private const string VidPidRegexFormat = ".*VID_{0:X}.*PID_{1:X}";

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="deviceProperties">Device Properties.</param>
        public BaseDeviceEvent(IDictionary<string, object> deviceProperties)
        {
            Description = GetValue(deviceProperties, "DeviceDesc", "");
            DeviceId = GetValue(deviceProperties, nameof(DeviceId), "");
            DeviceCategory = GetValue(deviceProperties, nameof(DeviceCategory), "");
        }

        /// <summary>
        ///     Description of the target instance for device.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Device Id of the target instance for the device.
        /// </summary>
        public string DeviceId { get; }

        /// <summary>
        ///     Device category e.g. USB, HID, DISPLAY
        /// </summary>
        public string DeviceCategory { get; }

        /// <summary>
        ///     Checks if the event is for given VendorId and ProductId.
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public bool IsForVidPid(int vendorId, int productId)
        {
            return Regex.IsMatch(
                DeviceId,
                string.Format(VidPidRegexFormat, vendorId, productId),
                RegexOptions.IgnoreCase);
        }

        private static T GetValue<T>(IDictionary<string, object> properties, string property, T defaultValue)
        {
            object value = defaultValue;
            return properties?.TryGetValue(property, out value) ?? false ? (T)value : defaultValue;
        }
    }
}