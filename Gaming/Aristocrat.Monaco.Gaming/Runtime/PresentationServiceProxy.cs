namespace Aristocrat.Monaco.Gaming.Runtime
{
    using Client;
    using Contracts;
    using System;
    using System.Collections.Generic;
    using Contracts.Events;
    using Kernel;

    public class PresentationServiceProxy : IPresentationService
    {
        private readonly IClientEndpointProvider<IPresentationService> _serviceProvider;
        private readonly IEventBus _eventBus;

        public PresentationServiceProxy(IClientEndpointProvider<IPresentationService> serviceProvider, IEventBus eventBus)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public bool Connected => _serviceProvider.Client?.Connected ?? false;

        public void PresentOverriddenPresentation(IList<PresentationOverrideData> presentations)
        {
            _serviceProvider.Client?.PresentOverriddenPresentation(presentations);
            _eventBus.Publish(new PresentationOverrideDataChangedEvent(presentations));
        }
    }
}
