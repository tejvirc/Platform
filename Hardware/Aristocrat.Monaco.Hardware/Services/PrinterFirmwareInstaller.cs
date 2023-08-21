namespace Aristocrat.Monaco.Hardware.Services
{
    using Aristocrat.Monaco.Kernel.Contracts;
    using Contracts;
    using Contracts.SharedDevice;
    using Kernel.Contracts.Components;
    using System;
    using System.Collections.Generic;

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
            : base(DeviceType.Printer, Constants.PrinterPath)
        {
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}
