namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Accounting.Contracts;
    using Commands;
    using Contracts;
    using Contracts.Barkeeper;
    using Kernel;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="BankBalanceChangedEvent" />
    /// </summary>
    public class BankBalanceChangedConsumer : Kernel.Consumes<BankBalanceChangedEvent>
    {
        private readonly IGamePlayState _gameState;
        private readonly ICommandHandlerFactory _handlerFactory;
        private readonly IBarkeeperHandler _barkeeperHandler;
        private readonly IPlayerBank _playerBank;
        private readonly IRuntime _runtime;

        public BankBalanceChangedConsumer(
            IEventBus eventBus,
            IRuntime runtime,
            IPlayerBank playerBank,
            IGamePlayState gameState,
            ICommandHandlerFactory handlerFactory,
            IBarkeeperHandler barkeeperHandler)
            : base(eventBus)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
        }

        public override void Consume(BankBalanceChangedEvent theEvent)
        {
            // Bail if the balance wasn't updated
            if (theEvent.NewBalance == theEvent.OldBalance)
            {
                return;
            }

            _barkeeperHandler.OnBalanceUpdate(theEvent.NewBalance);

            //  We're using the uncommitted state here due to the contention around adding credits and game start
            var currentState = _gameState.UncommittedState;

            // To handle money in while in a game round, send the update if the balance increases in the following states
            if (theEvent.NewBalance >= theEvent.OldBalance &&
                (currentState == PlayState.PrimaryGameStarted || currentState == PlayState.SecondaryGameStarted))
            {
                _runtime.UpdateBalance(_playerBank.Credits);

                return;
            }

            if (currentState != PlayState.Idle && currentState != PlayState.PresentationIdle)
            {
                return;
            }

            //**** Send new Credit Balance to GDKRuntime
            _runtime.UpdateBalance(_playerBank.Credits);

            if (theEvent.NewBalance == 0)
            {
                // Clear the session based local storage when reaching a zero balance
                _handlerFactory.Create<ClearSessionData>().Handle(new ClearSessionData());
            }
        }
    }
}
