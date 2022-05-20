namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.SharedDevice;

    public class IdReaderDisabledConsumer : Consumes<DisabledEvent>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _command;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public IdReaderDisabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> command,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(DisabledEvent theEvent)
        {
            if ((theEvent.Reasons & DisabledReasons.Backend) == DisabledReasons.Backend)
            {
                var device = _egm.GetDevice<IIdReaderDevice>(theEvent.IdReaderId);

                var status = new idReaderStatus();
                _command.Build(device, status);
                _eventLift.Report(device, EventCode.G2S_IDE003, device.DeviceList(status));
            }
        }
    }
}
