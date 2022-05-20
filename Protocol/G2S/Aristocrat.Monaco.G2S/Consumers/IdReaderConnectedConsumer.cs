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

    public class IdReaderConnectedConsumer : Consumes<ConnectedEvent>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _command;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public IdReaderConnectedConsumer(
            IG2SEgm egm,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> command,
            IEventLift eventLift)
        {
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public override void Consume(ConnectedEvent theEvent)
        {
            var device = _egm.GetDevice<IIdReaderDevice>(theEvent.IdReaderId);
            if (device == null)
            {
                return;
            }

            var status = new idReaderStatus();
            _command.Build(device, status);

            _eventLift.Report(device, EventCode.G2S_IDE902, device.DeviceList(status));
             
            _eventLift.Report(device, EventCode.G2S_IDE002, device.DeviceList(status));
        }
    }
}
