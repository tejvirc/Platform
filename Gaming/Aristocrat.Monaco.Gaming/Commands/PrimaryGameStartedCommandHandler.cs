namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Accounting.Contracts.HandCount;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Accounting.HandCount;
    using CefSharp.Internals;
    using Contracts;
    using Contracts.Events;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Progressives;
    using Runtime;

    /// <summary>
    ///     Command handler for the <see cref="PrimaryGameStarted" /> command.
    /// </summary>
    public class PrimaryGameStartedCommandHandler : ICommandHandler<PrimaryGameStarted>
    {
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IGameHistory _gameHistory;
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IHandCount _handCount;
        private readonly IPersistentStorageManager _storage;
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly IPlayerBank _bank;
        private readonly IRuntime _runtime;

        public PrimaryGameStartedCommandHandler(
            IPropertiesManager properties,
            IEventBus eventBus,
            IPlayerBank bank,
            IRuntime runtime,
            IHandCount handCount,
            IGameHistory gameHistory,
            IPersistentStorageManager storage,
            IProgressiveGameProvider progressiveGame,
            ICommandHandlerFactory commandFactory)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _handCount = handCount;
        }

        /// <inheritdoc />
        public void Handle(PrimaryGameStarted command)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                _progressiveGame.IncrementProgressiveLevel(string.Empty, command.Wager.CentsToMillicents(), 0L);

                // Start before subtracting bet to get correct StartCredits.
                _gameHistory.Start(command.Wager, command.Data, _progressiveGame.GetJackpotSnapshot(string.Empty));

                _commandFactory.Create<Wager>().Handle(new Wager(command.GameId, command.Denomination, command.Wager));

                //assumption this will be only trigger for the jurisdiction
                _handCount.IncrementHandCount();

                scope.Complete();

                _runtime.UpdateBalance(_bank.Credits);
                //_runtime.UpdateParameters();
                //_runtime.UpdateHandCount(_handCount.HandCount);

            }

            if (_properties.GetValue(GamingConstants.AllowCashInDuringPlay, false))
            {
                _bank.Unlock();

                _eventBus.Publish(new AllowMoneyInEvent());
            }
        }
    }
}