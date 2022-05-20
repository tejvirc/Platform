namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Runtime;
    using Runtime.Client;

    public class AllowGameRoundChangedConsumer : Consumes<AllowGameRoundChangedEvent>
    {
        private readonly IGamePlayState _gamePlayState;
        private readonly IRuntime _runtime;

        public AllowGameRoundChangedConsumer(IGamePlayState gamePlayState, IRuntime runtime)
        {
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public override void Consume(AllowGameRoundChangedEvent theEvent)
        {
            if (!_runtime.Connected)
            {
                return;
            }

            if (!theEvent.AllowGameRound && (_gamePlayState.Idle || _gamePlayState.InPresentationIdle))
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}