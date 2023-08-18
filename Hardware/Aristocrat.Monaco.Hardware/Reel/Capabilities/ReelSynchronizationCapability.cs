namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Kernel;

    internal sealed class ReelSynchronizationCapability : IReelSynchronizationCapabilities
    {
        private readonly IEventBus _eventBus;
        private readonly ISynchronizationImplementation _implementation;

        public ReelSynchronizationCapability(ISynchronizationImplementation implementation, IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));
            
            _implementation.SynchronizationCompleted += HandleSynchronizationCompleted;
            _implementation.SynchronizationStarted += HandleSynchronizationStarted;
        }

        public void Dispose()
        {
            _implementation.SynchronizationCompleted -= HandleSynchronizationCompleted;
            _implementation.SynchronizationStarted -= HandleSynchronizationStarted;
        }

        public Task<bool> Synchronize(ReelSynchronizationData syncData, CancellationToken token = default)
        {
            return _implementation.Synchronize(syncData, token);
        }

        private void HandleSynchronizationCompleted(object sender, ReelSynchronizationEventArgs args)
        {
            _eventBus.Publish(new ReelSynchronizationEvent(args.ReelId, SynchronizeStatus.Complete));
        }

        private void HandleSynchronizationStarted(object sender, ReelSynchronizationEventArgs args)
        {
            _eventBus.Publish(new ReelSynchronizationEvent(args.ReelId, SynchronizeStatus.Started));
        }
    }
}
