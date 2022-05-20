namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Defines a contract for controlling a device
    /// </summary>
    public interface IDeviceControl
    {
        /// <summary>
        ///     Gets the command queue for the device.
        /// </summary>
        ICommandQueue Queue { get; }

        /// <summary>
        ///     Signifies that communication has just opened.
        /// </summary>
        /// <param name="context">Startup data provided to the device.</param>
        void Open(IStartupContext context);

        /// <summary>
        ///     Signifies that communication is about to be closed.
        /// </summary>
        void Close();

        /// <summary>
        ///     Registers events for the device.
        /// </summary>
        void RegisterEvents();

        /// <summary>
        ///     Unregister events for the device.
        /// </summary>
        void UnregisterEvents();
    }
}