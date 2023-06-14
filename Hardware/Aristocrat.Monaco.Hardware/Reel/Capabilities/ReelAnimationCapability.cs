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

        /// <summary>
        ///     Instantiates a new instance of the ReelAnimationCapability class
        /// </summary>
        /// <param name="implementation"></param>
        /// <param name="stateManager"></param>
        public ReelAnimationCapability(IAnimationImplementation implementation, ReelControllerStateManager stateManager)
        {
            _implementation = implementation;
            _stateManager = stateManager;
        }

        public IReadOnlyCollection<AnimationFile> AnimationFiles => _implementation.AnimationFiles;

        /// <inheritdoc />
        public Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token = default)
        {
            return _implementation.LoadAnimationFile(file, token);
        }

        /// <inheritdoc />
        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token = default)
        {
            return _implementation.LoadAnimationFiles(files, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimation(LightShowData showData, CancellationToken token = default)
        {
            return _implementation.PrepareAnimation(showData, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            return _implementation.PrepareAnimations(showData, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimation(ReelCurveData curveData, CancellationToken token = default)
        {
            return _implementation.PrepareAnimation(curveData, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token = default)
        {
            return _implementation.PrepareAnimations(curveData, token);
        }

        /// <inheritdoc />
        public Task<bool> PlayAnimations(CancellationToken token = default)
        {
            return _implementation.PlayAnimations(token);
        }

        /// <inheritdoc />
        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            return _implementation.StopLightShowAnimations(showData, token);
        }

        /// <inheritdoc />
        public Task<bool> StopAllLightShows(CancellationToken token = default)
        {
            return _implementation.StopAllLightShows(token);
        }

        /// <inheritdoc />
        public Task<bool> StopAllAnimationTags(string animationName, CancellationToken token = default)
        {
            return _implementation.StopAllAnimationTags(animationName, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token = default)
        {
            return _implementation.PrepareStopReels(stopData, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token = default)
        {
            return _implementation.PrepareNudgeReels(nudgeData, token);
        }
    }
}
