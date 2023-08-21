namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Storage;

    /// <summary>
    ///     Uninstall module arguments
    /// </summary>
    public class UninstallModuleArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UninstallModuleArgs" /> class.
        /// </summary>
        /// <param name="moduleEntity">The module entity.</param>
        /// <param name="uninstallModuleCallback">Uninstall module callback</param>
        public UninstallModuleArgs(Module moduleEntity, Action<UninstallModuleArgs> uninstallModuleCallback)
        {
            ModuleEntity = moduleEntity ?? throw new ArgumentNullException(nameof(moduleEntity));
            UninstallModuleCallback = uninstallModuleCallback;
        }

        /// <summary>
        ///     Gets the module entity.
        /// </summary>
        /// <value>
        ///     The module entity.
        /// </value>
        public Module ModuleEntity { get; }

        /// <summary>
        ///     Gets uninstall module callback
        /// </summary>
        public Action<UninstallModuleArgs> UninstallModuleCallback { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether or not an active device was changed while installing or uninstalling
        /// </summary>
        public bool DeviceChanged { get; set; }
    }
}