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

        public HashSet<AnimationData> AnimationFiles => _communicator.AnimationFiles;

        public Task<bool> LoadAnimationFile(AnimationData data, CancellationToken token)
        {
            return _communicator.LoadControllerAnimationFile(data, token);
        }

        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationData> files, CancellationToken token)
        {
            return _communicator.LoadControllerAnimationFiles(files, token);
        }

        public Task<bool> PrepareAnimation(LightShowData data, CancellationToken token)
        {
            return _communicator.PrepareControllerAnimation(data, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<LightShowData> files, CancellationToken token)
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

        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowData> files, CancellationToken token)
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

        public Task<bool> RemoveAllAnimations(CancellationToken token)
        {
            return _communicator.RemoveAllControllerAnimations(token);
        }
    }
}
