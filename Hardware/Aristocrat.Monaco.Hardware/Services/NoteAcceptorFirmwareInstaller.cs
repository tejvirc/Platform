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
    public class NoteAcceptorFirmwareInstaller : FirmwareInstallerBase, INoteAcceptorFirmwareInstaller
    {
        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(INoteAcceptorFirmwareInstaller) };

        public NoteAcceptorFirmwareInstaller()
            : this(
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IComponentRegistry>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IDfuProvider>())
        {
        }

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
        public void Initialize()
        {
        }
    }
}
