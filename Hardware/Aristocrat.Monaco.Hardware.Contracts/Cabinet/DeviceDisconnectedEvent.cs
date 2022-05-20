namespace Aristocrat.Monaco.Hardware.Contracts.Cabinet
{
    using System.Collections.Generic;

    /// <summary>
    ///     A plug and play device disconnected event.
    /// </summary>
    public class DeviceDisconnectedEvent : BaseDeviceEvent
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="deviceProperties">Device Properties.</param>
        public DeviceDisconnectedEvent(IDictionary<string, object> deviceProperties)
            : base(deviceProperties)
        {
        }
    }
}