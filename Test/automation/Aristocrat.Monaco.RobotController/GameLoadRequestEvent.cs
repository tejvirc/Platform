namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;

    internal class GameLoadRequestEvent : BaseEvent
    {
        public bool GoToNextGame{ get; private set; }
        public GameLoadRequestEvent()
        {
        }
        public GameLoadRequestEvent(bool nextGame)
        {
            GoToNextGame = nextGame;
        }
    }
}
