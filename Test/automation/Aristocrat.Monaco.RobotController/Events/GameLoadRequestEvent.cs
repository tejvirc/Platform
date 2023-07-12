namespace Aristocrat.Monaco.RobotController
{
    using Aristocrat.Monaco.Kernel;

    internal class GameLoadRequestEvent : BaseEvent
    {
        public bool SelectNextGame { get; set; }
    }
}
