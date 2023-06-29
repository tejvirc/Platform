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
    public class NoteAcceptorFirmwareInstaller : FirmwareInstallerBase, INoteAcceptorFirmwareInstaller
    {
        public NoteAcceptorFirmwareInstaller(
            IPathMapper pathMapper,
            IComponentRegistry componentRegistry,
            IEventBus eventBus,
            IDfuProvider dfuProvider)
            : base(
                DeviceType.NoteAcceptor,
                Constants.NoteAcceptorPath,
                pathMapper,
                componentRegistry,
                eventBus,
                dfuProvider)
        {
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(INoteAcceptorFirmwareInstaller) };

        /// <inheritdoc />
        public void Initialize()
        {
        }
    }
}