namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Client;
    using GdkRuntime.V1;

    public class ReelServiceProxy : IReelService
    {
        private readonly IClientEndpointProvider<IReelService> _serviceProvider;

        public ReelServiceProxy(IClientEndpointProvider<IReelService> serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        
        /// <inheritdoc />
        public bool Connected => _serviceProvider.Client?.Connected ?? false;
        
        /// <inheritdoc />
        public void UpdateReelState(IDictionary<int, ReelLogicalState> updateData)
        {
            _serviceProvider.Client?.UpdateReelState(updateData);
        }
        
        /// <inheritdoc />
        public void NotifyAnimationUpdated(AnimationUpdatedNotification updateData)
        {
            _serviceProvider.Client?.NotifyAnimationUpdated(updateData);
        }

        /// <inheritdoc />
        public void NotifyStepperRuleTriggered(int reelId, int eventId)
        {
            _serviceProvider.Client?.NotifyStepperRuleTriggered(reelId, eventId);
        }

        /// <inheritdoc />
        public void NotifyReelSynchronizationStatus(int reelId, SynchronizeStatus status)
        {
            _serviceProvider.Client?.NotifyReelSynchronizationStatus(reelId, status);
        }
        
        /// <inheritdoc />
        public void NotifyReelStopping(int reelId, long timeToStop)
        {
            _serviceProvider.Client?.NotifyReelStopping(reelId, timeToStop);
        }
    }
}