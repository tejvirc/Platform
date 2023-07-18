namespace Aristocrat.Monaco.Hardware.Contracts
{
    using SharedDevice;
    using System;

    /// <summary>
    ///     Use this attribute to tag hardware devices for automatic discovery and registration 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HardwareDeviceAttribute : Attribute
    {
        /// <summary>
        ///     Gets the communication protocol used by this device
        /// </summary>
        public string Protocol { get; }

        /// <summary>
        ///     Gets the device type
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareDeviceAttribute"/> class
        /// </summary>
        /// <param name="protocol">Sets the communication protocol</param>
        /// <param name="deviceType">Sets the <see cref="DeviceType"/></param>
        public HardwareDeviceAttribute(string protocol, DeviceType deviceType)
        {
            Protocol = protocol;
            DeviceType = deviceType;
        }
    }
}