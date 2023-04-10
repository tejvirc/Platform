namespace Aristocrat.Monaco.Hardware.Serial.Protocols
{
    using System;
    using Contracts.SharedDevice;

    /// <summary>
    ///     This attribute is used to tag serial protocol classes that are
    ///     meant to be used during searches for serial devices.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SearchableSerialProtocolAttribute : Attribute
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="deviceType">which device type</param>
        /// <param name="maxWaitForResponse">max wait (ms)</param>
        public SearchableSerialProtocolAttribute(DeviceType deviceType, int maxWaitForResponse = 0)
        {
            DeviceType = deviceType;
            MaxWaitForResponse = maxWaitForResponse;
        }

        /// <summary>
        ///     Which device type
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        ///     How long to wait (maximum) for a detection response from device after the initial probe.
        /// </summary>
        public int MaxWaitForResponse { get; }
    }
}
