namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Diagnostics;
    using Application.Contracts.Extensions;
    using Common.PerformanceCounters;
    using Contracts;
    using Contracts.Meters;
    using Hardware.Contracts.Persistence;
    using Kernel;

    [CounterDescription("Secondary Game End", PerformanceCounterType.AverageTimer32)]
    public class SecondaryGameEndedCommandHandler : ICommandHandler<SecondaryGameEnded>
    {
        private readonly IGameHistory _gameHistory;
        private readonly IGameMeterManager _gameMeters;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;

        public SecondaryGameEndedCommandHandler(
            IGameHistory gameHistory,
            IGameMeterManager gameMeters,
            IPropertiesManager properties,
            IPersistentStorageManager storage)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _gameMeters = gameMeters ?? throw new ArgumentNullException(nameof(gameMeters));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public void Handle(SecondaryGameEnded command)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                // Start before subtracting bet to get correct StartCredits.
                _gameHistory.EndSecondaryGame(command.Win);

                var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
                var denom = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

                if (command.Win == command.Stake) // Tied
                {
                    _gameMeters.GetMeter(gameId, denom, GamingMeters.SecondaryWonAmount).Increment(command.Win.CentsToMillicents());
                    _gameMeters.GetMeter(gameId, denom, GamingMeters.SecondaryTiedCount).Increment(1);
                }
                else if (command.Win > 0) // Player won
                {
                    _gameMeters.GetMeter(gameId, denom, GamingMeters.SecondaryWonAmount).Increment(command.Win.CentsToMillicents());
                    _gameMeters.GetMeter(gameId, denom, GamingMeters.SecondaryWonCount).Increment(1);
                }
                else // Player lost
                {
                    _gameMeters.GetMeter(gameId, denom, GamingMeters.SecondaryLostCount).Increment(1);
                }

                scope.Complete();
            }
        }
    }
}
