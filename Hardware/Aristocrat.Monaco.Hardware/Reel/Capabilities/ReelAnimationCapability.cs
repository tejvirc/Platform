namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.ImplementationCapabilities;

    internal class ReelAnimationCapability : IReelAnimationCapabilities
    {
        private readonly IAnimationImplementation _implementation;
        private readonly ReelControllerStateManager _stateManager;

        public ReelAnimationCapability(IAnimationImplementation implementation, ReelControllerStateManager stateManager)
        {
            _implementation = implementation;
            _stateManager = stateManager;
        }

        public Task<bool> LoadControllerAnimationFile(AnimationFile file, CancellationToken token)
        {
            return _implementation.LoadAnimationFile(file, token);
        }

        public Task<bool> LoadControllerAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token)
        {
            return _implementation.LoadAnimationFiles(files, token);
        }

        public Task<bool> RemoveAllControllerAnimations(CancellationToken token)
        {
            return _implementation.RemoveAllAnimations(token);
        }

        public Task<bool> PrepareControllerAnimation(LightShowFile file, CancellationToken token)
        {
            return _implementation.PrepareAnimation(file, token);
        }

        public Task<bool> PrepareControllerAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            return _implementation.PrepareAnimations(files, token);
        }

        public Task<bool> PrepareControllerAnimation(ReelCurveData curveData, CancellationToken token)
        {
            return _implementation.PrepareAnimation(curveData, token);
        }

        public Task<bool> PrepareControllerAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token)
        {
            return _implementation.PrepareAnimations(curveData, token);
        }

        public Task<bool> PlayControllerAnimations(CancellationToken token)
        {
            return _implementation.PlayAnimations(token);
        }

        public Task<bool> StopControllerLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            return _implementation.StopLightShowAnimations(files, token);
        }

        public Task<bool> StopAllControllerLightShows(CancellationToken token)
        {
            return _implementation.StopAllLightShows(token);
        }

        public Task<bool> PrepareControllerStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            return _implementation.PrepareStopReels(stopData, token);
        }

        public Task<bool> PrepareControllerNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            return _implementation.PrepareNudgeReels(nudgeData, token);
        }
    }
}
