namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Linq;
    using Contracts;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Progressives;

    /// <summary>
    ///     A connect jackpot pool command handler.
    /// </summary>
    /// <seealso
    ///     cref="T:Aristocrat.Monaco.Gaming.Commands.ICommandHandler{Aristocrat.Monaco.Gaming.Commands.ConnectJackpotPool}" />
    public class ConnectJackpotPoolCommandHandler : ICommandHandler<ConnectJackpotPool>
    {
        private readonly IPropertiesManager _properties;
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IPersistentStorageManager _storage;
        private readonly IProgressiveErrorProvider _progressiveError;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectJackpotPoolCommandHandler" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        public ConnectJackpotPoolCommandHandler(
            IPropertiesManager properties,
            IProgressiveGameProvider progressiveGame,
            IPersistentStorageManager storage,
            IProgressiveErrorProvider progressiveError)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _progressiveError = progressiveError ?? throw new ArgumentNullException(nameof(progressiveError));
        }

        /// <inheritdoc />
        public void Handle(ConnectJackpotPool command)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
            var betOption = _properties.GetValue(GamingConstants.SelectedBetOption, String.Empty);

            using (var scope = _storage.ScopedTransaction())
            {
                var levels = _progressiveGame.ActivateProgressiveLevels(command.PoolName, gameId, denomId, betOption);

                command.Connected = levels.Any();
                _progressiveError.CheckProgressiveLevelErrors(levels);

                scope.Complete();
            }
        }
    }
}