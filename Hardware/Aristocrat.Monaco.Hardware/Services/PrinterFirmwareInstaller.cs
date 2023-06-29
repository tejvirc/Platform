namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Dfu;
    using Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts;
    using Kernel.Contracts.Components;

    /// <summary>
    ///     An <see cref="IInstaller" /> implementation for firmware
    /// </summary>
    public class PrinterFirmwareInstaller : FirmwareInstallerBase, IPrinterFirmwareInstaller
    {
        public PrinterFirmwareInstaller(
            IPathMapper pathMapper,
            IComponentRegistry componentRegistry,
            IEventBus eventBus,
            IDfuProvider dfuProvider)
            : base(
                DeviceType.Printer,
                Constants.PrinterPath,
                pathMapper,
                componentRegistry,
                eventBus,
                dfuProvider)
        {
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPrinterFirmwareInstaller) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}