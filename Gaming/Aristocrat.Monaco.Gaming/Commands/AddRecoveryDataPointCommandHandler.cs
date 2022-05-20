namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;

    /// <summary>
    ///     Command handler for the <see cref="AddRecoveryDataPoint" /> command.
    /// </summary>
    public class AddRecoveryDataPointCommandHandler : ICommandHandler<AddRecoveryDataPoint>
    {
        private readonly IGameHistory _gameHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddRecoveryDataPointCommandHandler" /> class.
        /// </summary>
        /// <param name="gameHistory">An <see cref="IGameHistory" /> instance.</param>
        public AddRecoveryDataPointCommandHandler(IGameHistory gameHistory)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        /// <inheritdoc />
        public void Handle(AddRecoveryDataPoint command)
        {
            // Must be Synchronous
            _gameHistory.SaveRecoveryPoint(command.Data);
        }
    }
}
