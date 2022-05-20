namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;

    /// <summary>
    ///     Command handler for the <see cref="SetGameResult" /> command.
    /// </summary>
    public class SetGameResultCommandHandler : ICommandHandler<SetGameResult>
    {
        private readonly IGameHistory _gameHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetGameResultCommandHandler" /> class.
        /// </summary>
        /// <param name="gameHistory">An <see cref="IGameHistory" /> instance.</param>
        public SetGameResultCommandHandler(
            IGameHistory gameHistory)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        /// <inheritdoc />
        public void Handle(SetGameResult command)
        {
            _gameHistory.Results(command.Win);
        }
    }
}
