namespace Aristocrat.Monaco.RobotController
{
    using log4net;

    internal class RobotLogger
    {
        private readonly StateChecker _stateChecker;
        private readonly ILog _logger;
        public RobotLogger(StateChecker stateChecker, ILog logger)
        {
            _stateChecker = stateChecker;
            _logger = logger;
        }
        public void Info(string msg, string className)
        {
            _logger.Info($"Controller: {_stateChecker._lobbyStateManager.CurrentState} - GamePlayState: {_stateChecker._gamePlayState.CurrentState} -[{className}]: {msg}");
        }

        public void Error(string msg, string className)
        {
            _logger.Error($"Controller: {_stateChecker._lobbyStateManager.CurrentState} - GamePlayState: {_stateChecker._gamePlayState.CurrentState} -[{className}]: {msg}");
        }

        public void Fatal(string msg, string className)
        {
            _logger.Fatal($"Controller: {_stateChecker._lobbyStateManager.CurrentState} - GamePlayState: {_stateChecker._gamePlayState.CurrentState} -[{className}]: {msg}");
        }
    }
}
