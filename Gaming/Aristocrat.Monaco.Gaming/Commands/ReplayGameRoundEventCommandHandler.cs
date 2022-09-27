namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using GameRound;
    using Runtime.Client;
    using RuntimeEvents;

    public class ReplayGameRoundEventCommandHandler : ICommandHandler<ReplayGameRoundEvent>
    {
        private readonly IReplayRuntimeEventHandler _eventHandler;
        private readonly IGameRoundInfoParserFactory _parserFactory;

        public ReplayGameRoundEventCommandHandler(
            IReplayRuntimeEventHandler eventHandler,
            IGameRoundInfoParserFactory parserFactory)
        {
            _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
            _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
        }

        public void Handle(ReplayGameRoundEvent command)
        {
            _eventHandler.HandleEvent(command);
            if (command.Action is GameRoundEventAction.Triggered && command.GameRoundInfo?.Count > 0)
            {
                _parserFactory.UpdateGameRoundInfo(command.GameRoundInfo);
            }
        }
    }
}
