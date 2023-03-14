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
        public SearchableSerialProtocolAttribute(DeviceType deviceType)
        {
            DeviceType = deviceType;
        }

        public DeviceType DeviceType { get; }
    }
}
