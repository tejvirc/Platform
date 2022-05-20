namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     Provides a mechanism to interact with the HardwareHelpers class that isn't static
    /// </summary>
    public interface IHardwareHelper : IService
    {
        /// <summary>
        ///     Checks is virtual button deck hardware exists.
        /// </summary>
        /// <returns>True if the virtual button deck exists as 3rd monitor.</returns>
        bool CheckForVirtualButtonDeckHardware();

        /// <summary>
        ///     Checks is usb button deck hardware exists.
        /// </summary>
        /// <returns>True if the usb button deck exists.</returns>
        bool CheckForUsbButtonDeckHardware();
    }
}
