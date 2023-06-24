namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Handlers;

    public class StartSessionFailedEventEventConsumer : Consumes<StartSessionFailedEvent>
    {
        private readonly ICommandBuilder<IIdReaderDevice, idReaderStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public StartSessionFailedEventEventConsumer(
            IG2SEgm egm,
            IEventLift eventLift,
            ICommandBuilder<IIdReaderDevice, idReaderStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public override void Consume(StartSessionFailedEvent theEvent)
        {
            var device = _egm.GetDevice<IIdReaderDevice>(theEvent.ReaderId);

            var status = new idReaderStatus();
            _commandBuilder.Build(device, status);

            _eventLift.Report(device, EventCode.G2S_PRE200, device.DeviceList(status), theEvent);
        }
    }
}