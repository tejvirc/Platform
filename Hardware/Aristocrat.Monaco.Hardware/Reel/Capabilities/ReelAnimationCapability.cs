﻿namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System;
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

        public Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareAnimation(LightShowFile file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareAnimation(ReelCurveData file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PlayAnimations(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopAllLightShows(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
