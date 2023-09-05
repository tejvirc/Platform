namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Contracts;
    using log4net;
    using Runtime;
    using Runtime.Client;

    public class AllowGameRoundChangedConsumer : Consumes<AllowGameRoundChangedEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

            Logger.Debug($"AllowGameRound={theEvent.AllowGameRound}, _gamePlayState.CurrentState={_gamePlayState.CurrentState}, ShowingMaxWinWarning={_maxWinOverlayService.ShowingMaxWinWarning}");
            if (!theEvent.AllowGameRound && (_gamePlayState.Idle || _gamePlayState.InPresentationIdle) && !_maxWinOverlayService.ShowingMaxWinWarning)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}