namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    using System;
    using Contracts;
    using Kernel;
    using Runtime.Client;

    public class WaitingForPlayerInputEventHandler : IRuntimeEventHandler
    {
        private readonly IEventBus _eventBus;

        public WaitingForPlayerInputEventHandler(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void HandleEvent(GameRoundEvent gameRoundEvent)
        {
            switch (gameRoundEvent.Action)
            {
                case GameRoundEventAction.Begin:
                    _eventBus.Publish(new WaitingForPlayerInputStartedEvent());
                    break;
                case GameRoundEventAction.Completed:
                    _eventBus.Publish(new WaitingForPlayerInputEndedEvent());
                    break;
            }
        }
    }
}