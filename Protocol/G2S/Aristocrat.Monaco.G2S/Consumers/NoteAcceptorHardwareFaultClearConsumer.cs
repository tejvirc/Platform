namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using System;

    /// <summary>
    ///     Handles the NoteAcceptor <see cref="NoteAcceptorHardwareFaultClearConsumer"/>
    /// </summary>
    public class NoteAcceptorHardwareFaultClearConsumer : Consumes<HardwareFaultClearEvent>
    {
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IDeviceRegistryService _deviceRegistry;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorHardwareFaultClearConsumer"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="egm">An <see cref="IG2SEgm"/> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}"/> implementation.</param>
        /// <param name="eventLift">A G2S event lift.</param>
        /// <param name="deviceRegistry">The device registry.</param>
        public NoteAcceptorHardwareFaultClearConsumer(
            IG2SEgm egm,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
            IEventLift eventLift,
            IDeviceRegistryService deviceRegistry)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
        }

    /// <summary>
    ///     Handle hardware fault events
    /// </summary>
    /// <param name="theEvent">The Event to handle</param>
    public override void Consume(HardwareFaultClearEvent theEvent)
        {
            var device = _egm.GetDevice<INoteAcceptorDevice>(theEvent.NoteAcceptorId);
            if (device == null)
            {
                return;
            }

            var status = new noteAcceptorStatus();
            _commandBuilder.Build(device, status);

            foreach (NoteAcceptorFaultTypes value in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if ((theEvent.Fault & value) != value)
                {
                    continue;
                }

                string eventCode;
                switch (value)
                {
                    case NoteAcceptorFaultTypes.StackerDisconnected:
                        eventCode = EventCode.G2S_NAE104;
                        break;
                    default:
                        continue;
                }

                _eventLift.Report(
                    device,
                    eventCode,
                    device.DeviceList(status),
                    theEvent);
            }

            var noteAcceptor = _deviceRegistry.GetDevice<INoteAcceptor>();
            if ((noteAcceptor?.Faults ?? NoteAcceptorFaultTypes.None) == NoteAcceptorFaultTypes.None)
            {
                _eventLift.Report(
                    device,
                    EventCode.G2S_NAE099,
                    device.DeviceList(status),
                    theEvent);
            }
        }
    }
}
