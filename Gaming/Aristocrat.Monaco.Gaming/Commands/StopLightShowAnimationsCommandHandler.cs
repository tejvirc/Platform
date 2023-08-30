namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="StopLightShowAnimations" /> command.
    /// </summary>
    public class StopLightShowAnimationsCommandHandler : ICommandHandler<StopLightShowAnimations>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelAnimationCapabilities _animationCapabilities;

        /// <summary>
        ///     Creates a new instance of the <see cref="StopLightShowAnimationsCommandHandler"/> class.
        /// </summary>
        public StopLightShowAnimationsCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelAnimationCapabilities>() ?? false)
            {
                _animationCapabilities = reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(StopLightShowAnimations command)
        {
            Logger.Debug("Handle StopLightShowAnimations command");

            if (_animationCapabilities is not null)
            {
                var result = _animationCapabilities.StopLightShowAnimations(command.LightShowData);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}
