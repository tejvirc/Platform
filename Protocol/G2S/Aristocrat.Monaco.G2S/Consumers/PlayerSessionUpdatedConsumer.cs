namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Session;
    using Handlers;
    using Handlers.Player;

    public class PlayerSessionUpdatedConsumer : Consumes<SessionUpdatedEvent>
    {
        private readonly ICommandBuilder<IPlayerDevice, playerStatus> _command;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        public PlayerSessionUpdatedConsumer(
            IG2SEgm egm,
            ICommandBuilder<IPlayerDevice, playerStatus> command,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public override void Consume(SessionUpdatedEvent theEvent)
        {
            var device = _egm.GetDevice<IPlayerDevice>();
            if (device == null)
            {
                return;
            }

            var status = new playerStatus();
            _command.Build(device, status);

            _eventLift.Report(
                device,
                EventCode.G2S_PRE114,
                device.DeviceList(status),
                theEvent.Log.TransactionId,
                device.TransactionList(theEvent.Log.ToPlayerLog(device)),
                null);
        }
    }
}
