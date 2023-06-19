namespace Aristocrat.Monaco.RobotController.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class RobotRunStatus
    {
        private bool _isGameExitInProgress;
        private readonly object _gameExitingLock = new();

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
    }
}
