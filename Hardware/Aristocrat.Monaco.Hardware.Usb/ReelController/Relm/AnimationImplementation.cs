namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;

    internal class AnimationImplementation : IAnimationImplementation
    {
#pragma warning disable 67
        public event EventHandler<LightAnimationEventArgs> LightAnimationStarted;

        public event EventHandler<LightAnimationEventArgs> LightAnimationCompleted;

        public event EventHandler<LightAnimationEventArgs> ReelAnimationStarted;

        public event EventHandler<LightAnimationEventArgs> ReelAnimationCompleted;
#pragma warning restore 67

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
