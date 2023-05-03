namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;

    internal class ReelSynchronizationCapability : IReelSynchronizationCapabilities
    {
        public Task<bool> Synchronize(ReelSynchronizationData data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
