namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;

    internal class RelmAnimation : IAnimationImplementation
    {
        private readonly IRelmCommunicator _communicator;

        public RelmAnimation(IRelmCommunicator communicator)
        {
            _communicator = communicator;
        }

#pragma warning disable 67
        public event EventHandler<LightAnimationEventArgs> LightAnimationStarted;

        public event EventHandler<LightAnimationEventArgs> LightAnimationCompleted;

        public event EventHandler<LightAnimationEventArgs> ReelAnimationStarted;

        public event EventHandler<LightAnimationEventArgs> ReelAnimationCompleted;
#pragma warning restore 67

        public Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token)
        {
            return _communicator.LoadAnimationFile(file, token);
        }

        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token)
        {
            return _communicator.LoadAnimationFiles(files, token);
        }

        public Task<bool> PrepareAnimation(LightShowFile file, CancellationToken token)
        {
            return _communicator.PrepareAnimation(file, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            return _communicator.PrepareAnimations(files, token);
        }

        public Task<bool> PrepareAnimation(ReelCurveData file, CancellationToken token)
        {
            return _communicator.PrepareAnimation(file, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> files, CancellationToken token)
        {
            return _communicator.PrepareAnimations(files, token);
        }

        public Task<bool> PlayAnimations(CancellationToken token)
        {
            return _communicator.PlayAnimations(token);
        }

        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            return _communicator.StopLightShowAnimations(files, token);
        }

        public Task<bool> StopAllLightShows(CancellationToken token)
        {
            return _communicator.StopAllLightShows(token);
        }

        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            return _communicator.PrepareStopReels(stopData, token);
        }

        public Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            return _communicator.PrepareNudgeReels(nudgeData, token);
        }
    }
}
