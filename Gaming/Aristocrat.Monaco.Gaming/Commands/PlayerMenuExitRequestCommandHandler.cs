namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using Contracts;
    using Contracts.Events;
    using Kernel;

    /// <summary>
    ///     Command handler for the <see cref="PlayerMenuExitRequest" /> command.
    /// </summary>
    public class PlayerMenuExitRequestCommandHandler : ICommandHandler<PlayerMenuExitRequest>
    {
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerMenuExitRequestCommandHandler" /> class.
        /// </summary>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        public PlayerMenuExitRequestCommandHandler(
            IEventBus eventBus,
            IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public void Handle(PlayerMenuExitRequest command)
        {
            if (_properties.GetValue(GamingConstants.ShowPlayerMenuPopup, true))
            {
                //_eventBus.Publish(new PlayerMenuButtonPressedEvent(false));
            }
        }
    }
}