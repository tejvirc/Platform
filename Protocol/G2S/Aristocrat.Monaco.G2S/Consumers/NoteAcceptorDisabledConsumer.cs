namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;

    /// <summary>
    ///     Handles the Note Acceptor <see cref="DisabledEvent" />
    /// </summary>
    public class NoteAcceptorDisabledConsumer : Consumes<DisabledEvent>
    {
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _commandBuilder;
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorDisabledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{INoteAcceptorDevice,noteAcceptorStatus}" /> implementation</param>
        /// <param name="deviceRegistry">An <see cref="IDeviceRegistryService" /> instance.</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public NoteAcceptorDisabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
            IDeviceRegistryService deviceRegistry,
            IEventLift eventLift)
        {
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(DisabledEvent theEvent)
        {
            if ((theEvent.Reasons & DisabledReasons.Service) == DisabledReasons.Service ||
                (theEvent.Reasons & DisabledReasons.Error) == DisabledReasons.Error ||
                (theEvent.Reasons & DisabledReasons.FirmwareUpdate) == DisabledReasons.FirmwareUpdate ||
                (theEvent.Reasons & DisabledReasons.Device) == DisabledReasons.Device)
            {
                var noteAcceptor = _egm.GetDevice<INoteAcceptorDevice>();
                if (noteAcceptor == null || !noteAcceptor.Enabled)
                {
                    return;
                }

                noteAcceptor.Enabled = false;

                var device = _deviceRegistry.GetDevice<INoteAcceptor>();

                if (device?.StackerState == NoteAcceptorStackerState.Removed && !noteAcceptor.RequiredForPlay)
                {
                    _egm.GetDevice<ICabinetDevice>().AddCondition(
                        noteAcceptor,
                        EgmState.EgmDisabled,
                        (int)NoteAcceptorStackerState.Removed);
                }
                else
                {
                    _egm.GetDevice<ICabinetDevice>().AddCondition(noteAcceptor, EgmState.EgmDisabled);
                }

                var status = new noteAcceptorStatus();
                _commandBuilder.Build(noteAcceptor, status);
                _eventLift.Report(
                    noteAcceptor,
                    EventCode.G2S_NAE001,
                    noteAcceptor.DeviceList(status),
                    theEvent);
            }
        }
    }
}
