namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="NudgeReels" /> command.
    /// </summary>
    public class NudgeReelsCommandHandler : ICommandHandler<NudgeReels>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelSpinCapabilities _spinCapabilities;
        private readonly IReelAnimationCapabilities _animationCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NudgeReelsCommandHandler" /> class.
        /// </summary>
        public NudgeReelsCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelSpinCapabilities>() ?? false)
            {
                _spinCapabilities = reelController.GetCapability<IReelSpinCapabilities>();
            }

            if (reelController?.HasCapability<IReelAnimationCapabilities>() ?? false)
            {
                _animationCapabilities = reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(NudgeReels command)
        {
            Logger.Debug("Handle NudgeReels command");

            if (_spinCapabilities is not null)
            {
                var result = _spinCapabilities.NudgeReels(command.NudgeSpinData);
                command.Success = result.Result;
                return;
            }

            if (_animationCapabilities is not null)
            {
                var result = _animationCapabilities.PrepareNudgeReels(command.NudgeSpinData);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}