namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;

    using Contracts.Events;
    using Kernel;
    

    /// <summary>
    ///     Command handler for the <see cref="UpdateLanguage" /> command.
    /// </summary>
    public class UpdateLanguageRequestCommandHandler : ICommandHandler<UpdateLanguage>
    {
        private readonly IEventBus _eventBus;


        public UpdateLanguageRequestCommandHandler(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Handle(UpdateLanguage command)
        {
            _eventBus.Publish(new GameLanguageChangedEvent(command.LocaleCode));
        }
    }
}
