namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Session;
    using Handlers;
    using Handlers.Player;
    using Services;

    public class PlayerSessionEndedConsumer : Consumes<SessionEndedEvent>
    {
        private readonly ICommandBuilder<IPlayerDevice, playerStatus> _command;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IInformedPlayerService _ipService;
        private readonly IGameMeterManager _meters;

        public PlayerSessionEndedConsumer(
            IG2SEgm egm,
            ICommandBuilder<IPlayerDevice, playerStatus> command,
            IEventLift eventLift,
            IInformedPlayerService ipService,
            IGameMeterManager meters)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _ipService = ipService ?? throw new ArgumentNullException(nameof(ipService));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
        }

        public override void Consume(SessionEndedEvent theEvent)
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
                EventCode.G2S_PRE103,
                device.DeviceList(status),
                theEvent.Log.TransactionId,
                device.TransactionList(
                    theEvent.Log.ToPlayerLog(device),
                    theEvent.Log.ToPlayerMeterLog(
                        device,
                        device.SubscribedMeters.Expand(_egm.Devices),
                        (id, meterName) => _meters.GetMeterName(id, meterName))),
                null);

            _ipService.OnPlayerSessionEnded();
        }
    }
}
