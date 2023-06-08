namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Kernel;
    using System.Linq;

    /// <summary>
    ///     Command handler for the <see cref="StopAllAnimationTags" /> command.
    /// </summary>
    public class StopAllAnimationTagsCommandHandler : ICommandHandler<StopAllAnimationTags>
    {
        private readonly IReelAnimationCapabilities _animationCapabilities;

        public StopAllAnimationTagsCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelAnimationCapabilities>() ?? false)
            {
                _animationCapabilities = reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(StopAllAnimationTags command)
        {
            if (_animationCapabilities is null)
            {
                command.Success = false;
                return;
            }

            var success = _animationCapabilities.StopAllAnimationTags(
                (int)_animationCapabilities.AnimationFiles
                    .First( x => x .FriendlyName == command.AnimationName )
                    .AnimationId
                ).Result;

            command.Success = success;
        }
    }
}
