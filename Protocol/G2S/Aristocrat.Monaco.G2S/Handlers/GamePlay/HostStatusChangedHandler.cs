namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;

    public class HostStatusChangedHandler : IStatusChangedHandler<IGamePlayDevice>
    {
        private readonly ICommandBuilder<IGamePlayDevice, gamePlayStatus> _commandBuilder;
        private readonly IEventLift _eventLift;
        private readonly IGameProvider _gameProvider;

        public HostStatusChangedHandler(
            IGameProvider gameProvider,
            ICommandBuilder<IGamePlayDevice, gamePlayStatus> commandBuilder,
            IEventLift eventLift)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public void Handle(IGamePlayDevice device)
        {
            // The disabled event will be emitted in the GameDisabledConsumer
            if (device.HostEnabled)
            {
                _gameProvider.EnableGame(device.Id, GameStatus.DisabledByBackend);

                var status = new gamePlayStatus();
                _commandBuilder.Build(device, status);
                _eventLift.Report(
                    device,
                    EventCode.G2S_GPE004,
                    device.DeviceList(status));
            }
            else
            {
                _gameProvider.DisableGame(device.Id, GameStatus.DisabledByBackend);
            }
        }
    }
}