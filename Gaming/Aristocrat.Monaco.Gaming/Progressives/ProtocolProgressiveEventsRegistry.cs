namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Protocol;
    using Common;
    using Contracts.Progressives;
    using Kernel;

    /// <inheritdoc cref="IProtocolProgressiveEventsRegistry" />
    public class ProtocolProgressiveEventsRegistry : IProtocolProgressiveEventsRegistry
    {
        private readonly IEventBus _eventBus;
        private readonly IMultiProtocolConfigurationProvider _multiProtocolConfigurationProvider;

        /// <summary>
        /// </summary>
        /// <param name="eventBus"></param>
        /// <param name="multiProtocolConfigurationProvider"></param>
        public ProtocolProgressiveEventsRegistry(
            IEventBus eventBus,
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider)
        {
            _eventBus = eventBus;
            _multiProtocolConfigurationProvider = multiProtocolConfigurationProvider;
        }

        /// <summary>
        /// </summary>
        public ProtocolProgressiveEventsRegistry()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>())
        {
        }

        private string ProtocolHandlingProgressives
        {
            get
            {
                var protocol = _multiProtocolConfigurationProvider.MultiProtocolConfiguration
                    .SingleOrDefault(x => x.IsProgressiveHandled)?.Protocol;

                return protocol.HasValue ? EnumParser.ToName(protocol) : null;
            }
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IProtocolProgressiveEventsRegistry) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void SubscribeProgressiveEvent<T>(string protocolName, IProtocolProgressiveEventHandler handler)
            where T : IEvent
        {
            if (ProtocolHandlingProgressives != protocolName)
            {
                return;
            }

            _eventBus.Subscribe<T>(handler, handler.HandleProgressiveEvent);
        }

        public void UnSubscribeProgressiveEvent<T>(string protocolName, IProtocolProgressiveEventHandler handler)
            where T : IEvent
        {
            if (ProtocolHandlingProgressives != protocolName || handler == null)
            {
                return;
            }

            _eventBus.Unsubscribe<T>(handler);
        }

        public void PublishProgressiveEvent(string protocolName, IEvent @event)
        {
            if (ProtocolHandlingProgressives != protocolName)
            {
                return;
            }

            _eventBus.Publish(@event);
        }
    }
}