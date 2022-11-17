namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Runtime;
    using Contracts.Events;
    using Kernel;
    using Runtime.Client;

    /// <summary>
    ///     Command handler for the <see cref="UpdateLanguage" /> command.
    /// </summary>
    public class UpdateLanguageRequestCommandHandler : ICommandHandler<UpdateLanguage>
    {
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly IRuntime _runtime;

        public UpdateLanguageRequestCommandHandler(IPropertiesManager properties, IEventBus eventBus, IRuntime runtime)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void Handle(UpdateLanguage command)
        {
            _eventBus.Publish(new GameLanguageChangedEvent(command.LocaleCode));

            _runtime.UpdateState(RuntimeState.LanguageUpdate);
        }
    }
}
