namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.ImplementationCapabilities;

    internal class ReelSynchronizationCapability : IReelSynchronizationCapabilities
    {
        private readonly ISynchronizationImplementation _implementation;
        private readonly ReelControllerStateManager _stateManager;

        public ReelSynchronizationCapability(ISynchronizationImplementation implementation, ReelControllerStateManager stateManager)
        {
            _implementation = implementation;
            _stateManager = stateManager;
        }

        public Task<bool> Synchronize(ReelSynchronizationData data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
