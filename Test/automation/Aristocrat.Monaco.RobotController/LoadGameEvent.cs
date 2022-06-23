namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;

    internal class LoadGameEvent : BaseEvent
    {
        public bool GoToNextGame{ get; private set; }

        public LoadGameEvent(bool nextGame = false)
        {
            GoToNextGame = nextGame;
        }
    }
}
