namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Defines a host-oriented device.  The device identifiers for host-oriented devices MUST be equal to the host
    ///     identifier of the host that owns the device.
    /// </summary>
    /// <typeparam name="TClass">The class this device implements.</typeparam>
    public abstract class HostOrientedDevice<TClass> : ClientDeviceBase<TClass>
        where TClass : IClass, new()
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostOrientedDevice{TClass}" /> class.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        protected HostOrientedDevice(int deviceId, IDeviceObserver deviceStateObserver)
            : this(deviceId, deviceStateObserver, true)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostOrientedDevice{TClass}" /> class.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="deviceStateObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="hostEnabled">Sets the initial host enabled attribute.</param>
        protected HostOrientedDevice(int deviceId, IDeviceObserver deviceStateObserver, bool hostEnabled)
            : base(deviceId, deviceStateObserver, hostEnabled)
        {
            Owner = deviceId;
            Active = true;
        }
    }
}