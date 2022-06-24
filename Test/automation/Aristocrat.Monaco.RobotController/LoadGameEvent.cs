namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;

    internal class LoadGameEvent : BaseEvent
    {
        public bool GoToNextGame{ get; private set; }
        public LoadGameEvent()
        {
        }
        public LoadGameEvent(bool nextGame)
        {
            GoToNextGame = nextGame;
        }
    }
}
