namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.ImplementationCapabilities;

    internal sealed class ReelLightingCapability : IReelLightingCapabilities
    {
        private readonly IReelLightingImplementation _implementation;
        private readonly ReelControllerStateManager _stateManager;

        public ReelLightingCapability(IReelLightingImplementation implementation, ReelControllerStateManager stateManager)
        {
            _implementation = implementation;
            _stateManager = stateManager;
        }

        public Task<IList<int>> GetReelLightIdentifiers() => _implementation.GetReelLightIdentifiers();

        public void Dispose()
        {
        }

        public Task<bool> SetLights(params ReelLampData[] lampData)
        {
            return _stateManager.CanSendCommand ? _implementation.SetLights(lampData) : Task.FromResult(false);
        }
    }
}
