namespace Aristocrat.Monaco.Hardware.Contracts.Cabinet
{
    using System.Collections.Generic;

    /// <summary>
    ///     A plug and play device connected event.
    /// </summary>
    public class DeviceConnectedEvent : BaseDeviceEvent
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="deviceProperties">Device Properties.</param>
        public DeviceConnectedEvent(IReadOnlyDictionary<string, object> deviceProperties)
            : base(deviceProperties)
        {
        }
    }
}