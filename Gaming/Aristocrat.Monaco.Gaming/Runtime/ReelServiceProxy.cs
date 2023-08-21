namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Client;

    public class ReelServiceProxy : IReelService
    {
        private readonly IClientEndpointProvider<IReelService> _serviceProvider;

        public ReelServiceProxy(IClientEndpointProvider<IReelService> serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public bool Connected => _serviceProvider.Client?.Connected ?? false;

        public void UpdateReelState(IDictionary<int, ReelLogicalState> updateData)
        {
            _serviceProvider.Client?.UpdateReelState(updateData);
        }
    }
}