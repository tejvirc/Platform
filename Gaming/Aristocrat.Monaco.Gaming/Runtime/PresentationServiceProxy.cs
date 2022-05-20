namespace Aristocrat.Monaco.Gaming.Runtime
{
    using Client;
    using Contracts;
    using System;
    using System.Collections.Generic;

    public class PresentationServiceProxy : IPresentationService
    {
        private readonly IClientEndpointProvider<IPresentationService> _serviceProvider;

        public PresentationServiceProxy(IClientEndpointProvider<IPresentationService> serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public bool Connected => _serviceProvider.Client?.Connected ?? false;

        public void PresentOverriddenPresentation(IList<PresentationOverrideData> presentations)
        {
            _serviceProvider.Client?.PresentOverriddenPresentation(presentations);
        }
    }
}
