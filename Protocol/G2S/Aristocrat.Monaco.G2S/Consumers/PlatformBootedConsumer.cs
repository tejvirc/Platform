namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Profile;
    using Handlers;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="PlatformBootedEvent" />
    /// </summary>
    public class PlatformBootedConsumer : Consumes<PlatformBootedEvent>
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IProfileService _profiles;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlatformBootedConsumer" /> class with a G2S Engine.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        /// <param name="profiles">An <see cref="IProfileService" /> instance.</param>
        public PlatformBootedConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift,
            IProfileService profiles)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        }

        /// <inheritdoc />
        public override void Consume(PlatformBootedEvent theEvent)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                return;
            }

            var status = new cabinetStatus();
            _commandBuilder.Build(cabinet, status);

            _eventLift.Report(cabinet, EventCode.G2S_CBE325, cabinet.DeviceList(status));

            if (cabinet.ProcessorReset)
            {
                _eventLift.Report(cabinet, EventCode.G2S_CBE402);

                cabinet.ProcessorReset = false;
                _profiles.Save(cabinet);
            }

            if (theEvent.CriticalMemoryCleared)
            {
                _eventLift.Report(cabinet, EventCode.G2S_CBE321);
                _eventLift.Report(cabinet, EventCode.G2S_CBE322);
            }
        }
    }
}