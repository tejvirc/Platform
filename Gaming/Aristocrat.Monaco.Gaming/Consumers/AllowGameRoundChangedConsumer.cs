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
        private readonly IMaxWinOverlayService _maxWinOverlayService;

        public AllowGameRoundChangedConsumer(IGamePlayState gamePlayState, IRuntime runtime, IMaxWinOverlayService maxWinOverlayService)
        {
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _maxWinOverlayService = maxWinOverlayService ?? throw new ArgumentNullException(nameof(maxWinOverlayService));
        }

        public override void Consume(AllowGameRoundChangedEvent theEvent)
        {
            if (!_runtime.Connected)
            {
                return;
            }

            if (!theEvent.AllowGameRound && (_gamePlayState.Idle || _gamePlayState.InPresentationIdle) && !_maxWinOverlayService.ShowingMaxWinOverlayService)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}