namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using GameRound;
    using Runtime.Client;
    using RuntimeEvents;

    /// <summary>
    ///     Command handler for the <see cref="GameRoundEvent" /> command.
    /// </summary>
    public class GameRoundEventCommandHandler : ICommandHandler<GameRoundEvent>
    {
        private readonly IRuntimeEventHandlerFactory _eventHandlerFactory;
        private readonly IGameRoundInfoParserFactory _parserFactory;

        public GameRoundEventCommandHandler(
            IRuntimeEventHandlerFactory eventHandlerFactory,
            IGameRoundInfoParserFactory parserFactory)
        {
            _eventHandlerFactory = eventHandlerFactory ?? throw new ArgumentNullException(nameof(eventHandlerFactory));
            _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
        }

        public void Handle(GameRoundEvent command)
        {
            var handler = _eventHandlerFactory.Create(command.State);
            handler?.HandleEvent(command);
            if (command.Action is GameRoundEventAction.Triggered && command.GameRoundInfo?.Count > 0)
            {
                _parserFactory.UpdateGameRoundInfo(command.GameRoundInfo);
            }
        }
    }
}
