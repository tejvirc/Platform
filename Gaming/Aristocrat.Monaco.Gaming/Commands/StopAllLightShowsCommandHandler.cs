namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="StopAllLightShowAnimations" /> command.
    /// </summary>
    public class StopAllLightShowsCommandHandler : ICommandHandler<StopAllLightShowAnimations>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelAnimationCapabilities _animationCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StopAllLightShowsCommandHandler" /> class.
        /// </summary>
        public StopAllLightShowsCommandHandler()
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
            Logger.Debug("Handle StopAllLightShowAnimations command");

            if (_animationCapabilities is not null)
            {
                var result = _animationCapabilities.StopAllLightShows();
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}