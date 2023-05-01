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
            return _communicator.LoadControllerAnimationFile(file, token);
        }

        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token)
        {
            return _communicator.LoadControllerAnimationFiles(files, token);
        }

        public Task<bool> PrepareAnimation(LightShowFile file, CancellationToken token)
        {
            return _communicator.PrepareControllerAnimation(file, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            return _communicator.PrepareControllerAnimations(files, token);
        }

        public Task<bool> PrepareAnimation(ReelCurveData curveData, CancellationToken token)
        {
            return _communicator.PrepareControllerAnimation(curveData, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> files, CancellationToken token)
        {
            return _communicator.PrepareControllerAnimations(files, token);
        }

        public Task<bool> PlayAnimations(CancellationToken token)
        {
            return _communicator.PlayControllerAnimations(token);
        }

        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowFile> files, CancellationToken token)
        {
            return _communicator.StopControllerLightShowAnimations(files, token);
        }

        public Task<bool> StopAllLightShows(CancellationToken token)
        {
            return _communicator.StopAllControllerLightShows(token);
        }

        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token)
        {
            return _communicator.PrepareControllerStopReels(stopData, token);
        }

        public Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token)
        {
            return _communicator.PrepareControllerNudgeReels(nudgeData, token);
        }
    }
}
