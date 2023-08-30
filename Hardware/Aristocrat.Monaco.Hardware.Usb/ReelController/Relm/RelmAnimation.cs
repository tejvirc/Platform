namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;

    internal sealed class RelmAnimation : IAnimationImplementation
    {
        private readonly IRelmCommunicator _communicator;

        public RelmAnimation(IRelmCommunicator communicator)
        {
            _communicator = communicator;
            _communicator.AllLightAnimationsCleared += HandleAllLightAnimationsCleared;
            _communicator.LightAnimationRemoved += HandleLightAnimationRemoved;
            _communicator.LightAnimationStarted += HandleLightAnimationStarted;
            _communicator.LightAnimationStopped += HandleLightAnimationStopped;
            _communicator.LightAnimationPrepared += HandleLightAnimationPrepared;
            _communicator.ReelAnimationStarted += HandleReelAnimationStarted;
            _communicator.ReelAnimationStopped += HandleReelAnimationStopped;
            _communicator.ReelAnimationPrepared += HandleReelAnimationPrepared;
        }

        public event EventHandler AllLightAnimationsCleared;

        public event EventHandler<LightAnimationEventArgs> LightAnimationRemoved;

        public event EventHandler<LightAnimationEventArgs> LightAnimationStarted;

        public event EventHandler<LightAnimationEventArgs> LightAnimationStopped;

        public event EventHandler<LightAnimationEventArgs> LightAnimationPrepared;

        public event EventHandler<ReelAnimationEventArgs> ReelAnimationStarted;

        public event EventHandler<ReelAnimationEventArgs> ReelAnimationStopped;

        public event EventHandler<ReelAnimationEventArgs> ReelAnimationPrepared;

        public IReadOnlyCollection<AnimationFile> AnimationFiles => _communicator.AnimationFiles;

        public void Dispose()
        {
            _communicator.AllLightAnimationsCleared -= HandleAllLightAnimationsCleared;
            _communicator.LightAnimationRemoved -= HandleLightAnimationRemoved;
            _communicator.LightAnimationStarted -= HandleLightAnimationStarted;
            _communicator.LightAnimationStopped -= HandleLightAnimationStopped;
            _communicator.LightAnimationPrepared -= HandleLightAnimationPrepared;
            _communicator.ReelAnimationStarted -= HandleReelAnimationStarted;
            _communicator.ReelAnimationStopped -= HandleReelAnimationStopped;
            _communicator.ReelAnimationPrepared -= HandleReelAnimationPrepared;
        }

        public Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token = default)
        {
            return _communicator.LoadAnimationFile(file, token);
        }

        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, IProgress<LoadingAnimationFileModel> progress, CancellationToken token = default)
        {
            return _communicator.LoadAnimationFiles(files, progress, token);
        }

        public Task<bool> PrepareAnimation(LightShowData showData, CancellationToken token = default)
        {
            return _communicator.PrepareAnimation(showData, token);
        }

        public Task<bool> PrepareAnimation(ReelCurveData curveData, CancellationToken token = default)
        {
            return _communicator.PrepareAnimation(curveData, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            return _communicator.PrepareAnimations(showData, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token = default)
        {
            return _communicator.PrepareAnimations(curveData, token);
        }

        public Task<bool> PlayAnimations(CancellationToken token = default)
        {
            return _communicator.PlayAnimations(token);
        }

        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            return _communicator.StopLightShowAnimations(showData, token);
        }

        public Task<bool> StopAllLightShows(CancellationToken token = default)
        {
            return _communicator.StopAllLightShows(token);
        }

        public Task<bool> StopAllAnimationTags(string animationName, CancellationToken token = default)
        {
            return _communicator.StopAllAnimationTags(animationName, token);
        }

        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token = default)
        {
            return _communicator.PrepareStopReels(stopData, token);
        }

        public Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token = default)
        {
            return _communicator.PrepareNudgeReels(nudgeData, token);
        }

        private void HandleAllLightAnimationsCleared(object sender, EventArgs args)
        {
            AllLightAnimationsCleared?.Invoke(sender, args);
        }

        private void HandleLightAnimationRemoved(object sender, LightAnimationEventArgs args)
        {
            LightAnimationRemoved?.Invoke(sender, args);
        }

        private void HandleLightAnimationStarted(object sender, LightAnimationEventArgs args)
        {
            LightAnimationStarted?.Invoke(sender, args);
        }

        private void HandleLightAnimationStopped(object sender, LightAnimationEventArgs args)
        {
            LightAnimationStopped?.Invoke(sender, args);
        }

        private void HandleLightAnimationPrepared(object sender, LightAnimationEventArgs args)
        {
            LightAnimationPrepared?.Invoke(sender, args);
        }

        private void HandleReelAnimationStarted(object sender, ReelAnimationEventArgs args)
        {
            ReelAnimationStarted?.Invoke(sender, args);
        }

        private void HandleReelAnimationStopped(object sender, ReelAnimationEventArgs args)
        {
            ReelAnimationStopped?.Invoke(sender, args);
        }

        private void HandleReelAnimationPrepared(object sender, ReelAnimationEventArgs args)
        {
            ReelAnimationPrepared?.Invoke(sender, args);
        }
    }
}
