namespace Aristocrat.Monaco.Hardware.Services
{
    using Aristocrat.Monaco.Kernel.Contracts;
    using Contracts;
    using Contracts.SharedDevice;
    using Kernel.Contracts.Components;
    using System;
    using System.Collections.Generic;
    using Contracts.Dfu;
    using Kernel;

    /// <summary>
    ///     An <see cref="IInstaller" /> implementation for firmware
    /// </summary>
    public class PrinterFirmwareInstaller : FirmwareInstallerBase, IPrinterFirmwareInstaller
    {
        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPrinterFirmwareInstaller) };

        public PrinterFirmwareInstaller()
            : this(
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IComponentRegistry>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IDfuProvider>())
        {
        }

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
        public void Initialize()
        {
        }
    }
}
