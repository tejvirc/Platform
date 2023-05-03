﻿namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;

    internal class ReelAnimationCapability : IReelAnimationCapabilities
    {
        public Task<bool> LoadControllerAnimationFile(AnimationFile file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> LoadControllerAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimation(LightShowFile file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimation(ReelCurveData file, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerAnimations(IEnumerable<ReelCurveData> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PlayControllerAnimations(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopControllerLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopAllControllerLightShows(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PrepareControllerNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}