namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Handlers;

    /// <summary>
    ///     Handles the <see cref="GameDisabledEvent" /> event.
    /// </summary>
    public class GameDisabledConsumer : Consumes<GameDisabledEvent>
    {
        private readonly ICommandBuilder<IGamePlayDevice, gamePlayStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDisabledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="gameProvider">An <see cref="IGameProvider" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        public GameDisabledConsumer(
            IG2SEgm egm,
            IGameProvider gameProvider,
            ICommandBuilder<IGamePlayDevice, gamePlayStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(GameDisabledEvent theEvent)
        {
            if (_gameProvider.GetAllGames().All(g => !g.Enabled))
            {
                var cabinet = _egm.GetDevice<ICabinetDevice>();
                cabinet?.AddCondition(cabinet, EgmState.EgmDisabled, (int)CabinetFaults.NoGamesEnabled);
            }

            var gamePlay = _egm.GetDevice<IGamePlayDevice>(theEvent.GameId);
            if (gamePlay == null)
            {
                return;
            }

            string eventCode;

            if (theEvent.Status == GameStatus.DisabledByBackend)
            {
                eventCode = EventCode.G2S_GPE003;
            }
            else
            {
                gamePlay.Enabled = false;
                eventCode = EventCode.G2S_GPE001;
            }

            var status = new gamePlayStatus();
            _commandBuilder.Build(gamePlay, status);
            _eventLift.Report(
                gamePlay,
                eventCode,
                gamePlay.DeviceList(status));
        }
    }
}
