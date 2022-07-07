namespace Aristocrat.Monaco.RobotController
{
    using log4net;
    using System.Reflection;

    internal class RobotLogger
    {
        private readonly StateChecker _stateChecker;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RobotLogger(StateChecker stateChecker)
        {
            _stateChecker = stateChecker;
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
