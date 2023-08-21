namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Diagnostics;
    using Common.PerformanceCounters;
    using Contracts;
    using Contracts.Barkeeper;
    using Contracts.Meters;
    using Hardware.Contracts.Persistence;
    using Kernel;

    [CounterDescription("Secondary Game Start", PerformanceCounterType.AverageTimer32)]
    public class SecondaryGameStartedCommandHandler : ICommandHandler<SecondaryGameStarted>
    {
        private readonly IGameHistory _gameHistory;
        private readonly IGameMeterManager _gameMeters;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly IBarkeeperHandler _barkeeperHandler;

        public SecondaryGameStartedCommandHandler(
            IGameHistory gameHistory,
            IGameMeterManager gameMeters,
            IPropertiesManager properties,
            IPersistentStorageManager storage,
            IBarkeeperHandler barkeeperHandler)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _gameMeters = gameMeters ?? throw new ArgumentNullException(nameof(gameMeters));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
        }

        public void Handle(SecondaryGameStarted command)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                // Start before subtracting bet to get correct StartCredits.
                _gameHistory.StartSecondaryGame(command.Stake);

                var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
                var denom = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

                var wageredAmount = command.Stake * GamingConstants.Millicents;
                _gameMeters.GetMeter(gameId, denom, GamingMeters.SecondaryWageredAmount).Increment(wageredAmount);
                _barkeeperHandler.CreditsWagered(wageredAmount);

                scope.Complete();
            }
        }
    }
}
