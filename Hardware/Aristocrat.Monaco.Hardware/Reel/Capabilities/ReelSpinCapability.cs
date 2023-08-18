namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Reel;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.ImplementationCapabilities;
    using log4net;

    internal sealed class ReelSpinCapability : IReelSpinCapabilities
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IReelSpinImplementation _implementation;
        private readonly ReelControllerStateManager _stateManager;

        public ReelSpinCapability(IReelSpinImplementation implementation, ReelControllerStateManager stateManager)
        {
            _implementation = implementation;
            _stateManager = stateManager;
        }

        public int DefaultSpinSpeed => _implementation.DefaultSpinSpeed;

        public void Dispose()
        {
        }

        public Task<bool> NudgeReels(params NudgeReelData[] reelData)
        {
            Logger.Debug("NudgeReels with stops called");
            var reelTriggers = reelData.Select(
                reel => (
                    reel.Direction == SpinDirection.Forward
                        ? ReelControllerTrigger.SpinReel
                        : ReelControllerTrigger.SpinReelBackwards, reel.ReelId));

            if (!_stateManager.Fire(reelTriggers))
            {
                return Task.FromResult(false);
            }

            return _implementation?.NudgeReels(reelData) ?? Task.FromResult(false);
        }

        public Task<bool> SpinReels(params ReelSpinData[] reelData) => SpinReelsInternal(reelData);

        public Task<bool> SetReelSpeed(params ReelSpeedData[] speedData)
        {
            return _stateManager.CanSendCommand ? _implementation.SetReelSpeed(speedData) : Task.FromResult(false);
        }

        private async Task<bool> SpinReelsInternal(ReelSpinData[] reelData)
        {
            Logger.Debug("SpinReels with stops called");
            var reelTriggers = reelData.Select(
                reel => (
                    reel.Direction == SpinDirection.Forward
                        ? ReelControllerTrigger.SpinReel
                        : ReelControllerTrigger.SpinReelBackwards, reel.ReelId));
            if (!_stateManager.Fire(reelTriggers))
            {
                return false;
            }

            return await (_implementation?.SpinReels(reelData) ?? Task.FromResult(false));
        }
    }
}
