namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts.Events;
    using Kernel;

    public class AttractModeEndedCommandHandler : ICommandHandler<AttractModeEnded>
    {
        private readonly IEventBus _eventBus;

        public AttractModeEndedCommandHandler(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Handle(AttractModeEnded command)
        {
            _eventBus.Publish(new AttractModeExited());
        }
    }
}