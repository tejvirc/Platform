namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Central;
    using Hardware.Contracts.Persistence;
    using Kernel;

    public class PrimaryGameEscrowCommandHandler : ICommandHandler<PrimaryGameEscrow>
    {
        private readonly ICentralProvider _central;
        private readonly IGameHistory _gameHistory;
        private readonly IGameRecovery _recovery;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;

        public PrimaryGameEscrowCommandHandler(
            ICentralProvider central,
            IGameHistory gameHistory,
            IGameRecovery recovery,
            IPropertiesManager properties,
            IPersistentStorageManager storage)
        {
            _central = central ?? throw new ArgumentNullException(nameof(central));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public void Handle(PrimaryGameEscrow command)
        {
            var (game, denomination) = _properties.GetActiveGame();

            using (var scope = _storage.ScopedTransaction())
            {
                // When recovering we can't update/create the log, but we need to ensure that the outcome request has been processed
                if (!_recovery.IsRecovering)
                {
                    _gameHistory.Escrow(command.InitialWager, command.Data);
                }

                // this is null when the game is not running
                if (game == null)
                {
                    return;
                }

            command.Result = command.Request switch
            {
                //*** If current PrimaryGameEscrow is valid
                OutcomeRequest request => _central.RequestOutcomes(
                    game.Id,
                    denomination.Value,
                    wagerCategory?.Id ?? string.Empty,
                    request.TemplateId.ToString(),
                    command.InitialWager.CentsToMillicents(),
                    command.Request,
                    _recovery.IsRecovering),
                _ => throw new NotSupportedException()
            };

                scope.Complete();
            }
        }
    }
}