namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts.Events;
    using Kernel;

    public class AttractModeStartedCommandHandler : ICommandHandler<AttractModeStarted>
    {
        private readonly IEventBus _eventBus;

        public AttractModeStartedCommandHandler(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Handle(AttractModeStarted command)
        {
            _eventBus.Publish(new AttractModeEntered());
        }
    }
}