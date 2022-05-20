namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.NoteAcceptor;
    using Meters;

    /// <summary>
    ///     Handles the Note Acceptor <see cref="HardwareFaultEvent" />
    /// </summary>
    public class NoteAcceptorHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IMetersSubscriptionManager _metersSubscriptionManager;
        private readonly IMeterAggregator<INoteAcceptorDevice> _meters;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorHardwareFaultConsumer" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{INoteAcceptorDevice,noteAcceptorStatus}" />
        /// implementation.</param>
        /// <param name="eventLift">A G2S event lift.</param>
        /// <param name="meters">The meters.</param>
        /// <param name="metersSubscriptionManager">Manager for meters subscription.</param>
        public NoteAcceptorHardwareFaultConsumer(
            IG2SEgm egm,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
            IEventLift eventLift,
            IMeterAggregator<INoteAcceptorDevice> meters,
            IMetersSubscriptionManager metersSubscriptionManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _metersSubscriptionManager = metersSubscriptionManager ?? throw new ArgumentNullException(nameof(metersSubscriptionManager));
        }

        /// <inheritdoc />
        public override void Consume(HardwareFaultEvent theEvent)
        {
            var device = _egm.GetDevice<INoteAcceptorDevice>();
            if (device == null)
            {
                return;
            }

            var status = new noteAcceptorStatus();
            _commandBuilder.Build(device, status);
            foreach (NoteAcceptorFaultTypes value in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (!theEvent.Fault.HasFlag(value))
                {
                    continue;
                }

                string eventCode;
                meterList meters = null;

                switch (value)
                {
                    case NoteAcceptorFaultTypes.FirmwareFault:
                        eventCode = EventCode.G2S_NAE903;
                        break;
                    case NoteAcceptorFaultTypes.MechanicalFault:
                        eventCode = EventCode.G2S_NAE904;
                        break;
                    case NoteAcceptorFaultTypes.OpticalFault:
                        eventCode = EventCode.G2S_NAE905;
                        break;
                    case NoteAcceptorFaultTypes.ComponentFault:
                        eventCode = EventCode.G2S_NAE906;
                        break;
                    case NoteAcceptorFaultTypes.NvmFault:
                        eventCode = EventCode.G2S_NAE907;
                        break;
                    case NoteAcceptorFaultTypes.CheatDetected:
                        eventCode = EventCode.G2S_NAE908;
                        break;
                    case NoteAcceptorFaultTypes.OtherFault:
                        eventCode = EventCode.G2S_NAE101;
                        break;
                    case NoteAcceptorFaultTypes.NoteJammed:
                        eventCode = EventCode.G2S_NAE101;
                        break;
                    case NoteAcceptorFaultTypes.StackerDisconnected:
                        eventCode = EventCode.G2S_NAE103;
                        meters = new meterList {
                            meterInfo = _meters.GetMeters(device, CurrencyMeterName.StackerRemovedCount).ToArray()
                        };
                        break;
                    case NoteAcceptorFaultTypes.StackerFull:
                        eventCode = EventCode.G2S_NAE105;
                        break;
                    case NoteAcceptorFaultTypes.StackerJammed:
                        eventCode = EventCode.G2S_NAE106;
                        break;
                    case NoteAcceptorFaultTypes.StackerFault:
                        eventCode = EventCode.G2S_NAE107;
                        break;
                    default:
                        continue;
                }

                _eventLift.Report(device,
                    eventCode,
                    device.DeviceList(status),
                    meters);

                if (meters != null)
                {
                    _metersSubscriptionManager.SendEndOfDayMeters(onNoteDrop: true);
                }
            }
        }
    }
}