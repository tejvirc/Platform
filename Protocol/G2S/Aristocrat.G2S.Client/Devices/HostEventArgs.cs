namespace Aristocrat.G2S.Client.Devices
{
    using System;

    /// <summary>
    ///     Base event for a host
    /// </summary>
    public class HostEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostEventArgs" /> class with the
        ///     specified
        ///     <i>hostId</i> and
        ///     <i>deviceId</i>.
        /// </summary>
        /// <param name="hostId">The ID of the host which originated the event.</param>
        /// <param name="deviceId">The ID of the device which originated the event.</param>
        public HostEventArgs(int hostId, int deviceId)
        {
            HostId = hostId;
            DeviceId = deviceId;
        }

        /// <summary>
        ///     Gets get the ID of the EGM which originated the event.
        /// </summary>
        public int HostId { get; }

        /// <summary>
        ///     Gets get the ID of the device which originated the event.
        /// </summary>
        public int DeviceId { get; }
    }
}