namespace Aristocrat.Monaco.Hardware.Reel.Capabilities
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Reel;
    using Contracts.Reel.Capabilities;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;
    using Kernel;

    internal sealed class ReelAnimationCapability : IReelAnimationCapabilities
    {
        private readonly IEventBus _eventBus;
        private readonly IAnimationImplementation _implementation;

        public ReelAnimationCapability(IAnimationImplementation implementation, IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _implementation = implementation ?? throw new ArgumentNullException(nameof(implementation));

            _implementation.AllLightAnimationsCleared += HandleAllLightAnimationsCleared;
            _implementation.LightAnimationRemoved += HandleLightAnimationRemoved;
            _implementation.LightAnimationStarted += HandleLightAnimationStarted;
            _implementation.LightAnimationStopped += HandleLightAnimationStopped;
            _implementation.LightAnimationPrepared += HandleLightAnimationPrepared;
            _implementation.ReelAnimationStarted += HandleReelAnimationStarted;
            _implementation.ReelAnimationStopped += HandleReelAnimationStopped;
            _implementation.ReelAnimationPrepared += HandleReelAnimationPrepared;
        }

        public IReadOnlyCollection<AnimationFile> AnimationFiles => _implementation.AnimationFiles;

        public void Dispose()
        {
            _implementation.AllLightAnimationsCleared -= HandleAllLightAnimationsCleared;
            _implementation.LightAnimationRemoved -= HandleLightAnimationRemoved;
            _implementation.LightAnimationStarted -= HandleLightAnimationStarted;
            _implementation.LightAnimationStopped -= HandleLightAnimationStopped;
            _implementation.LightAnimationPrepared -= HandleLightAnimationPrepared;
            _implementation.ReelAnimationStarted -= HandleReelAnimationStarted;
            _implementation.ReelAnimationStopped -= HandleReelAnimationStopped;
            _implementation.ReelAnimationPrepared -= HandleReelAnimationPrepared;
        }

        public Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token = default)
        {
            return _implementation.LoadAnimationFile(file, token);
        }

        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, IProgress<LoadingAnimationFileModel> progress, CancellationToken token = default)
        {
            return _implementation.LoadAnimationFiles(files, progress, token);
        }

        public Task<bool> PrepareAnimation(LightShowData showData, CancellationToken token = default)
        {
            return _implementation.PrepareAnimation(showData, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            return _implementation.PrepareAnimations(showData, token);
        }

        public Task<bool> PrepareAnimation(ReelCurveData curveData, CancellationToken token = default)
        {
            return _implementation.PrepareAnimation(curveData, token);
        }

        public Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token = default)
        {
            return _implementation.PrepareAnimations(curveData, token);
        }

        public Task<bool> PlayAnimations(CancellationToken token = default)
        {
            return _implementation.PlayAnimations(token);
        }

        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            return _implementation.StopLightShowAnimations(showData, token);
        }

        public Task<bool> StopAllLightShows(CancellationToken token = default)
        {
            return _implementation.StopAllLightShows(token);
        }

        public Task<bool> StopAllAnimationTags(string animationName, CancellationToken token = default)
        {
            return _implementation.StopAllAnimationTags(animationName, token);
        }

        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token = default)
        {
            return _implementation.PrepareStopReels(stopData, token);
        }

        public Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token = default)
        {
            return _implementation.PrepareNudgeReels(nudgeData, token);
        }

        private void HandleAllLightAnimationsCleared(object sender, EventArgs args)
        {
            _eventBus.Publish(new AllLightAnimationsClearedEvent());
        }

        private void HandleLightAnimationRemoved(object sender, LightAnimationEventArgs args)
        {
            _eventBus.Publish(new LightAnimationUpdatedEvent(args.AnimationName, args.Tag, AnimationState.Removed, args.QueueType));
        }

        private void HandleLightAnimationStarted(object sender, LightAnimationEventArgs args)
        {
            _eventBus.Publish(new LightAnimationUpdatedEvent(args.AnimationName, args.Tag, AnimationState.Started));
        }

        private void HandleLightAnimationStopped(object sender, LightAnimationEventArgs args)
        {
            _eventBus.Publish(new LightAnimationUpdatedEvent(args.AnimationName, args.Tag, AnimationState.Stopped, args.QueueType));
        }

        private void HandleLightAnimationPrepared(object sender, LightAnimationEventArgs args)
        {
            _eventBus.Publish(new LightAnimationUpdatedEvent(args.AnimationName, args.Tag, args.PreparedStatus));
        }

        private void HandleReelAnimationStarted(object sender, ReelAnimationEventArgs args)
        {
            _eventBus.Publish(new ReelAnimationUpdatedEvent(args.ReelId, args.AnimationName, AnimationState.Started));
        }

        private void HandleReelAnimationStopped(object sender, ReelAnimationEventArgs args)
        {
            _eventBus.Publish(new ReelAnimationUpdatedEvent(args.ReelId, args.AnimationName, AnimationState.Stopped));
        }

        private void HandleReelAnimationPrepared(object sender, ReelAnimationEventArgs args)
        {
            _eventBus.Publish(new ReelAnimationUpdatedEvent(args.AnimationName, args.PreparedStatus));
        }
    }
}
