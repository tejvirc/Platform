namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Reflection;
    using Contracts;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="AddRecoveryDataPoint" /> command.
    /// </summary>
    public class AddRecoveryDataPointCommandHandler : ICommandHandler<AddRecoveryDataPoint>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IGameHistory _gameHistory;
        private readonly IGamePlayState _gamePlayState;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AddRecoveryDataPointCommandHandler" /> class.
        /// </summary>
        /// <param name="gameHistory">An <see cref="IGameHistory" /> instance.</param>
        /// <param name="gamePlayState">An <see cref="IGamePlayState"/> instance.</param>
        public AddRecoveryDataPointCommandHandler(IGameHistory gameHistory, IGamePlayState gamePlayState)
        {
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
        }

        /// <inheritdoc />
        public void Handle(AddRecoveryDataPoint command)
        {
            if (_gamePlayState.Idle || _gamePlayState.InPresentationIdle)
            {
                Logger.Warn("Unable to set persistence not currently in a game round");
                return;
            }

            // Must be Synchronous
            _gameHistory.SaveRecoveryPoint(command.Data);
        }
    }
}
