namespace Aristocrat.Monaco.RobotController.Contracts
{
    using System;
    using Kernel;

    [Serializable]
    public class GameLoadRequestedEvent : BaseEvent
    {
        public int GameId;

        public long Denomination;
    }
}