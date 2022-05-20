namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Contracts.Meters;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Command handler for the <see cref="PrimaryGameEnded" /> command.
    /// </summary>
    public class PrimaryGameEndedCommandHandler : ICommandHandler<PrimaryGameEnded>
    {
        private readonly IGameHistory _gameHistory;
        private readonly IGameMeterManager _meterManager;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrimaryGameEndedCommandHandler" /> class.
        /// </summary>
        public PrimaryGameEndedCommandHandler(IGameHistory gameHistory, IPropertiesManager properties, IGameMeterManager meterManager, IPersistentStorageManager storage)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <inheritdoc />
        public void Handle(PrimaryGameEnded command)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                _meterManager.GetMeter(
                    _properties.GetValue(GamingConstants.SelectedGameId, 0),
                    _properties.GetValue(GamingConstants.SelectedDenom, 0L),
                    GamingMeters.PrimaryWonAmount)
                    .Increment(command.InitialWin * GamingConstants.Millicents);

                scope.Complete();
            }

            _gameHistory.EndPrimaryGame(command.InitialWin);
        }
    }
}
