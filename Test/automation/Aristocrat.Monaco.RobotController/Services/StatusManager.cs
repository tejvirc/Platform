namespace Aristocrat.Monaco.RobotController.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class StatusManager
    {
        private bool _isGameExitInProgress;
        private bool _goingNextGame;
        private bool _isGameRunning;
        private bool _isLoadGameInProgress;
        private bool _exitToLobbyWhenGameIdle;
        private bool _isTimeLimitDialogVisible;
        private byte _sanityCounter;

        private readonly object _gameExitingLock = new();
        private readonly object _nextGameLock = new();

        public bool IsGameExitInProgress
        {
            get => _isGameExitInProgress;
            set
            {
                lock (_gameExitingLock)
                {
                    _isGameExitInProgress = value;
                }
            }
        }

        public bool GoingNextGame
        {
            get => _goingNextGame;
            set
            {
                lock (_nextGameLock)
                {
                    _goingNextGame = value;
                }
            }
        }

        public bool IsGameRunning
        {
            get => _isGameRunning;
            set => _isGameRunning = value;
        }

        public bool IsLoadGameInProgress
        {
            get => _isLoadGameInProgress;
            set => _isLoadGameInProgress = value;
        }

        public bool ExitToLobbyWhenGameIdle
        {
            get => _exitToLobbyWhenGameIdle;
            set => _exitToLobbyWhenGameIdle = value;
        }

        public bool IsTimeLimitDialogVisible
        {
            get => _isTimeLimitDialogVisible;
            set => _isTimeLimitDialogVisible = value;
        }

        public byte SanityCounter
        {
            get => _sanityCounter;
            set
            {
                lock (this)
                {
                    _sanityCounter = value;
                }
            }
        }
    }
}
