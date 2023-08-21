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

    /// <summary>
    ///     Handles the <see cref="GameEnabledEvent" /> event.
    /// </summary>
    public class GameEnabledConsumer : Consumes<GameEnabledEvent>
    {
        private readonly ICommandBuilder<IGamePlayDevice, gamePlayStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEnabledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        public GameEnabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<IGamePlayDevice, gamePlayStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(GameEnabledEvent theEvent)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();
            cabinet?.RemoveCondition(cabinet, EgmState.EgmDisabled, (int)CabinetFaults.NoGamesEnabled);

            if (theEvent.Status == GameStatus.DisabledByBackend)
            {
                return;
            }

            var gamePlay = _egm.GetDevice<IGamePlayDevice>(theEvent.GameId);
            if (gamePlay == null)
            {
                return;
            }

            gamePlay.Enabled = true;

            var status = new gamePlayStatus();
            _commandBuilder.Build(gamePlay, status);
            _eventLift.Report(
                gamePlay,
                EventCode.G2S_GPE002,
                gamePlay.DeviceList(status),
                theEvent);
        }
    }
}
