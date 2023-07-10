namespace Aristocrat.Monaco.Gaming.Consumers
{
    using Commands;
    using Contracts;

    public class GamePlayStateInitializedConsumer : Consumes<GamePlayStateInitializedEvent>
    {
        private readonly IGameHistory _gameHistory;
        private readonly ICommandHandlerFactory _handlerFactory;

        public GamePlayStateInitializedConsumer(IGameHistory gameHistory, ICommandHandlerFactory handlerFactory)
        {
            _gameHistory = gameHistory;
            _handlerFactory = handlerFactory;
        }

        public override void Consume(GamePlayStateInitializedEvent theEvent)
        {
            if (!_gameHistory.IsRecoveryNeeded && theEvent.CurrentState == PlayState.Idle)
            {
                _handlerFactory.Create<CheckBalance>().Handle(new CheckBalance());
            }
        }
    }
}
