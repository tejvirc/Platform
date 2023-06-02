namespace Aristocrat.Monaco.Gaming.Commands
{
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;
    using System.Reflection;

    /// <summary>
    ///     Command handler for the <see cref="StartAnimations" /> command.
    /// </summary>
    public class StartAnimationsCommandHandler : ICommandHandler<StartAnimations>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelAnimationCapabilities _animationCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpinReelsCommandHandler" /> class.
        /// </summary>
        public StartAnimationsCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelAnimationCapabilities>() ?? false)
            {
                _animationCapabilities = reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(StartAnimations command)
        {
            Logger.Debug("Handle StartAnimations command");

            if (_animationCapabilities is not null)
            {
                var result = _animationCapabilities.PlayControllerAnimations(default);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}