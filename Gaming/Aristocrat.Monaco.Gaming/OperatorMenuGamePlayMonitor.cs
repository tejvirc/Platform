namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using log4net;

    public class OperatorMenuGamePlayMonitor : IOperatorMenuGamePlayMonitor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGamePlayState _gamePlayState;
        private readonly IGameHistory _gameHistory;
        private readonly IGameDiagnostics _gameDiagnostics;

        public OperatorMenuGamePlayMonitor(IGamePlayState gamePlayState, IGameHistory gameHistory, IGameDiagnostics gameDiagnostics)
        {
            _gamePlayState = gamePlayState;
            _gameHistory = gameHistory;
            _gameDiagnostics = gameDiagnostics;
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IOperatorMenuGamePlayMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Debug("Initialized the operator menu game play monitor.");
        }

        public bool InGameRound => _gamePlayState.InGameRound;

        public bool InReplay => _gameDiagnostics.IsActive;

        public bool IsRecoveryNeeded => _gameHistory.IsRecoveryNeeded;
    }
}
