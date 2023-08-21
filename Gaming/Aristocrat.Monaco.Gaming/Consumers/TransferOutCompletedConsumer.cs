namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Barkeeper;
    using log4net;
    using Runtime;
    using Runtime.Client;

    public class TransferOutCompletedConsumer : Consumes<TransferOutCompletedEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IGameHistory _history;
        private readonly IBarkeeperHandler _barkeeperHandler;
        private readonly ICashoutController _cashoutController;
        private readonly IGamePlayState _gameState;
        private readonly IPlayerBank _playerBank;
        private readonly IRuntime _runtime;

        public TransferOutCompletedConsumer(
            IGamePlayState gameState,
            IRuntime runtime,
            IPlayerBank playerBank,
            IGameHistory history,
            IBarkeeperHandler barkeeperHandler,
            ICashoutController cashoutController)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
            _cashoutController = cashoutController ?? throw new ArgumentNullException(nameof(cashoutController));
        }

        public override void Consume(TransferOutCompletedEvent theEvent)
        {
            _barkeeperHandler.OnCashOutCompleted();
            if (!_runtime.Connected)
            {
                return;
            }

            // This is intended to handle a cashout during a game round
            if (!_gameState.Idle)
            {
                _history.CompleteCashOut(theEvent.TraceId);

                _runtime.UpdateBalance(_playerBank.Credits);
                if (theEvent.Pending)
                {
                    return;
                }

                Logger.Debug("PendingHandpay set to false");
                _runtime.UpdateFlag(RuntimeCondition.PendingHandpay, false);

                _runtime.UpdateFlag(RuntimeCondition.AllowSubGameRound, true);

                if (_gameState.CurrentState == PlayState.PayGameResults)
                {
                    // This really belongs in the game play state machine, but there is contention on the state.  Need to consider moving this out of the event handler...
                    _gameState.End(-1);
                }
            }
            else if (!_cashoutController.PaperInChuteNotificationActive)
            {
                _runtime.UpdateFlag(RuntimeCondition.AllowGameRound, true);
            }
        }
    }
}