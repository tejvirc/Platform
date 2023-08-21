namespace Aristocrat.Monaco.G2S.Handlers.Player
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Session;

    public class HostStatusChangedHandler : IStatusChangedHandler<IPlayerDevice>
    {
        private readonly ICommandBuilder<IPlayerDevice, playerStatus> _command;
        private readonly IEventLift _eventLift;
        private readonly IPlayerService _playerService;

        public HostStatusChangedHandler(
            IPlayerService playerService,
            ICommandBuilder<IPlayerDevice, playerStatus> command,
            IEventLift eventLift)
        {
            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IPlayerDevice device)
        {
            if (device.HostEnabled)
            {
                _playerService.Enable(PlayerStatus.DisabledByBackend);
            }
            else
            {
                _playerService.Disable(PlayerStatus.DisabledByBackend);
            }

            var status = new playerStatus();
            _command.Build(device, status);
            _eventLift.Report(
                device,
                device.HostEnabled ? EventCode.G2S_PRE004 : EventCode.G2S_PRE003,
                device.DeviceList(status));
        }
    }
}