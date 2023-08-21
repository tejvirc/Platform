namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Handlers;

    /// <summary>
    ///     Handles the <see cref="GameDenomChangedEvent"/> event.
    /// </summary>
    public class GameDenomChangedByOperatorConsumer : Consumes<GameDenomChangedByOperatorEvent>
    {
        private readonly IG2SEgm _egm;
        private readonly ICommandBuilder<IGamePlayDevice, gamePlayStatus> _commandBuilder;
        private readonly IEventLift _eventLift;

        public GameDenomChangedByOperatorConsumer(
            IG2SEgm egm,
            ICommandBuilder<IGamePlayDevice, gamePlayStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm;
            _commandBuilder = commandBuilder;
            _eventLift = eventLift;
        }

        /// <inheritdoc />
        public override void Consume(GameDenomChangedByOperatorEvent theEvent)
        {
            var gamePlay = _egm.GetDevice<IGamePlayDevice>(theEvent.GameId);
            if (gamePlay == null)
            {
                return;
            }

            var status = new gamePlayStatus();
            _commandBuilder.Build(gamePlay, status);

            _eventLift.Report(gamePlay, EventCode.G2S_GPE201, gamePlay.DeviceList(status), theEvent);
            _eventLift.Report(gamePlay, EventCode.G2S_GPE006, gamePlay.DeviceList(status));
        }
    }
}
