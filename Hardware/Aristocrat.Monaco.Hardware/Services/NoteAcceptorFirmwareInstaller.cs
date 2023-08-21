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
    public class NoteAcceptorFirmwareInstaller : FirmwareInstallerBase, INoteAcceptorFirmwareInstaller
    {
        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(INoteAcceptorFirmwareInstaller) };

        public NoteAcceptorFirmwareInstaller()
            : base(DeviceType.NoteAcceptor, Constants.NoteAcceptorPath)
        {
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}
