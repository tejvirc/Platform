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

        /// <summary>
        ///     Instantiates a new instance of the RelmAnimation class
        /// </summary>
        /// <param name="communicator"></param>
        public RelmAnimation(IRelmCommunicator communicator)
        {
            _communicator = communicator;
        }

#pragma warning disable 67
        /// <inheritdoc />
        public event EventHandler<LightAnimationEventArgs> LightAnimationStarted;

        /// <inheritdoc />
        public event EventHandler<LightAnimationEventArgs> LightAnimationCompleted;

        /// <inheritdoc />
        public event EventHandler<LightAnimationEventArgs> ReelAnimationStarted;

        /// <inheritdoc />
        public event EventHandler<LightAnimationEventArgs> ReelAnimationCompleted;
#pragma warning restore 67

        /// <inheritdoc />
        public IReadOnlyCollection<AnimationFile> AnimationFiles => _communicator.AnimationFiles;

        /// <inheritdoc />
        public Task<bool> LoadAnimationFile(AnimationFile file, CancellationToken token = default)
        {
            return _communicator.LoadAnimationFile(file, token);
        }

        /// <inheritdoc />
        public Task<bool> LoadAnimationFiles(IEnumerable<AnimationFile> files, CancellationToken token = default)
        {
            return _communicator.LoadAnimationFiles(files, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimation(LightShowData showData, CancellationToken token = default)
        {
            return _communicator.PrepareAnimation(showData, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            return _communicator.PrepareAnimations(showData, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimation(ReelCurveData curveData, CancellationToken token = default)
        {
            return _communicator.PrepareAnimation(curveData, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareAnimations(IEnumerable<ReelCurveData> curveData, CancellationToken token = default)
        {
            return _communicator.PrepareAnimations(curveData, token);
        }

        /// <inheritdoc />
        public Task<bool> PlayAnimations(CancellationToken token = default)
        {
            return _communicator.PlayAnimations(token);
        }

        /// <inheritdoc />
        public Task<bool> StopLightShowAnimations(IEnumerable<LightShowData> showData, CancellationToken token = default)
        {
            return _communicator.StopLightShowAnimations(showData, token);
        }

        /// <inheritdoc />
        public Task<bool> StopAllLightShows(CancellationToken token = default)
        {
            return _communicator.StopAllLightShows(token);
        }

        /// <inheritdoc />
        public Task<bool> StopAllAnimationTags(string animationName, CancellationToken token = default)
        {
            return _communicator.StopAllAnimationTags(animationName, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareStopReels(IEnumerable<ReelStopData> stopData, CancellationToken token = default)
        {
            return _communicator.PrepareStopReels(stopData, token);
        }

        /// <inheritdoc />
        public Task<bool> PrepareNudgeReels(IEnumerable<NudgeReelData> nudgeData, CancellationToken token = default)
        {
            return _communicator.PrepareNudgeReels(nudgeData, token);
    }
}
}
