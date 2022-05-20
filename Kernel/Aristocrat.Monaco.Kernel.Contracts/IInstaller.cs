namespace Aristocrat.Monaco.Kernel.Contracts
{
    using System;

    /// <summary>
    ///     Provides a mechanism to interact with a product installer.
    /// </summary>
    public interface IInstaller
    {
        /// <summary>
        ///     Gets a value indicating whether or not an active device was changed while installing or uninstalling
        /// </summary>
        bool DeviceChanged { get; }

        /// <summary>
        ///     Gets the exit action for 
        /// </summary>
        ExitAction? ExitAction { get; }

        /// <summary>
        ///     Gets the event handler for the installer started.
        /// </summary>
        event EventHandler UninstallStartedEventHandler;

        /// <summary>
        ///     Installs the product
        /// </summary>
        /// <param name="packageId">The package identifier</param>
        /// <returns>true upon success, otherwise false</returns>
        bool Install(string packageId);

        /// <summary>
        ///     Uninstalls the product
        /// </summary>
        /// <param name="packageId">The package identifier</param>
        /// <returns>true if the product is uninstalled successfully, otherwise false</returns>
        bool Uninstall(string packageId);
    }
}