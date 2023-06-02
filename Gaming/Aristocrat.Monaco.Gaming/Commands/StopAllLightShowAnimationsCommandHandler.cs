namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;
    using System.Reflection;

    /// <summary>
    ///     Command handler for the <see cref="StopAllLightShowAnimations" /> command.
    /// </summary>
    public class StopAllLightShowAnimationsCommandHandler : ICommandHandler<StopAllLightShowAnimations>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelAnimationCapabilities _animationCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StopAllLightShowAnimations" /> class.
        /// </summary>
        public StopAllLightShowAnimationsCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelAnimationCapabilities>() ?? false)
            {
                _animationCapabilities = reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(StopAllLightShowAnimations command)
        {
            Logger.Debug("Handle StopAllLightshowAnimations command");

            if (_animationCapabilities is not null)
            {
                var result = _animationCapabilities.StopAllControllerLightShows(default);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}