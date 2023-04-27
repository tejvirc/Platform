namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using System.Threading.Tasks;
    using SharedDevice;

    /// <summary>
    ///     Interface to specify hardware device
    /// </summary>
    public interface IHardwareDevice
    {
        /// <summary>
        ///     Gets a value indicating whether the device is connected.
        /// </summary>
        /// <value>True if the device is connected, false if not.</value>
        bool IsConnected { get; }

        /// <summary>
        ///     Gets a value indicating whether the device is initialized.
        /// </summary>
        /// <returns>True or false</returns>
        bool IsInitialized { get; }

        /// <summary>
        ///     Gets a value indicating whether the device is enabled.
        /// </summary>
        /// <returns>True or false</returns>
        bool IsEnabled { get; }

        /// <summary>
        ///     Gets a the last calculated CRC.
        /// </summary>
        /// <value>The calculated CRC.</value>
        int Crc { get; }

        /// <summary>
        ///     Gets the protocol.
        /// </summary>
        /// <value>The protocol.</value>
        string Protocol { get; }

        /// <summary>
        ///     Event fired when device is initialized.
        /// </summary>
        event EventHandler<EventArgs> Initialized;

        /// <summary>
        ///     Event fired when device is initialization fails.
        /// </summary>
        event EventHandler<EventArgs> InitializationFailed;

        /// <summary>
        ///     Event fired when device is enabled.
        /// </summary>
        event EventHandler<EventArgs> Enabled;

        /// <summary>
        ///     Event fired when device is disabled.
        /// </summary>
        event EventHandler<EventArgs> Disabled;

        /// <summary>
        ///     Event fired when device is connected to the USB port.
        /// </summary>
        event EventHandler<EventArgs> Connected;

        /// <summary>
        ///     Event fired when device is disconnected to the USB port.
        /// </summary>
        event EventHandler<EventArgs> Disconnected;

        /// <summary>
        ///     Event fired when successfully reset a device.
        /// </summary>
        event EventHandler<EventArgs> ResetSucceeded;

        /// <summary>
        ///     Event fired when failed to reset a device.
        /// </summary>
        event EventHandler<EventArgs> ResetFailed;

        /// <summary>Opens the device.</summary>
        /// <returns>False if it fails.</returns>
        bool Open();

        /// <summary>Closes the device.</summary>
        /// <returns>False if it fails.</returns>
        bool Close();

        /// <summary>
        ///     Sends a command to the device to host enable the device.
        /// </summary>
        /// <remarks>
        ///     The device MUST activate any inputs and/or enable any outputs. If the device has a fault then it MUST stay
        ///     disabled and report the fault to the host.
        /// </remarks>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> Enable();

        /// <summary>
        ///     Sends a command to the device to host disable the device.
        /// </summary>
        /// <remarks>The device MUST deactivate any inputs and/or disable any outputs.</remarks>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> Disable();

        /// <summary>
        ///     Sends a command to the device to start a self test.
        /// </summary>
        /// <remarks>The device MUST initiate a self-test sequence when instructed by the host.</remarks>
        /// <param name="nvm">An flag indicating if Non-volatile Memory (NVM MUST be cleared before the self-test is performed.</param>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        Task<bool> SelfTest(bool nvm);

        /// <summary>
        ///     Sends a command to the device to initiate a 32-bit checksum calculation of the devices ROM memory.
        /// </summary>
        /// <returns>A Task (allows for asynchronous) with a calculated CRC.</returns>
        Task<int> CalculateCrc(int seed);

        /// <summary>
        ///     Updates the device configurations.
        /// </summary>
        /// <param name="internalConfiguration">Internal configuration to update.</param>
        void UpdateConfiguration(IDeviceConfiguration internalConfiguration);
    }
}
