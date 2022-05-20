namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Runtime;

    /// <summary>
    ///     Command handler for the <see cref="NotifyGameStarted" /> command.
    /// </summary>
    public class NotifyGameStartedCommandHandler : ICommandHandler<NotifyGameStarted>
    {
        private readonly IGameRecovery _gameRecovery;
        private readonly IPlayerBank _playerBank;
        private readonly IRuntime _runtime;

        public NotifyGameStartedCommandHandler(
            IRuntime runtime,
            IPlayerBank playerBank,
            IGameRecovery gameRecovery)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
        }

        /// <inheritdoc />
        public void Handle(NotifyGameStarted command)
        {
            if (_gameRecovery.IsRecovering)
            {
                // Don't update when recovering
                return;
            }

            _runtime.UpdateBalance(_playerBank.Credits);
        }
    }
}
