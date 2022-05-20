namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System.Threading.Tasks;
    using Communicator;

    /// <summary>Interface for device adapter.</summary>
    public interface IDeviceAdapter : IDeviceService, IFirmwareCrcBridge
    {
        /// <summary>
        ///     Gets the device configuration
        /// </summary>
        IDevice DeviceConfiguration { get; }

        /// <summary>
        ///     Gets the identifier of the product.
        /// </summary>
        /// <value>The identifier of the product.</value>
        int ProductId { get; }

        /// <summary>
        ///     Gets the identifier of the vendor.
        /// </summary>
        /// <value>The identifier of the vendor.</value>
        int VendorId { get; }

        /// <summary>Gets a value indicating whether or not the device is connected.</summary>
        bool Connected { get; }

        /// <summary>Gets a value indicating whether or not the device is disabled by an error reason.</summary>
        bool DisabledByError { get; }

        /// <summary>Gets the device type.</summary>
        /// <value>The device type.</value>
        DeviceType DeviceType { get; }

        /// <summary>
        ///     Inspects the device with the given communication settings.
        /// </summary>
        /// <param name="comConfiguration">communication settings used to connect to note acceptor.</param>
        /// <param name="timeout">time in milliseconds to notify of failed initialization if expired.</param>
        void Inspect(IComConfiguration comConfiguration, int timeout);

        /// <summary>
        ///     Sends a command to the device to start a self test.
        /// </summary>
        /// <remarks>The device MUST initiate a self-test sequence when instructed by the host.</remarks>
        /// <param name="clear">An flag indicating if memory should be cleared before the self-test is performed.</param>
        /// <returns>A Task (allows for asynchronous) with a GatData report.</returns>
        Task<bool> SelfTest(bool clear);
    }
}