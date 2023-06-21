namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="StopAllAnimationTags" /> command.
    /// </summary>
    public class StopAllAnimationTagsCommandHandler : ICommandHandler<StopAllAnimationTags>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
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
            Logger.Debug("Handle StopAllAnimationTags command");

            if (_animationCapabilities is not null)
            {
                var result = _animationCapabilities.StopAllAnimationTags(command.AnimationName);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}
