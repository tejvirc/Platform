namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     The PrepareStopReelsCommandHandler class
    /// </summary>
    public class PrepareStopReelsCommandHandler : ICommandHandler<PrepareStopReels>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelAnimationCapabilities _animationCapabilities;

        /// <summary>
        ///     Create a new instance of the <see cref="PrepareStopReelsCommandHandler" class./>
        /// </summary>
        public PrepareStopReelsCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelAnimationCapabilities>() ?? false)
            {
                _animationCapabilities = reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(PrepareStopReels command)
        {
            Logger.Debug("Handle PrepareStopReels command");

            if (_animationCapabilities is not null)
            {
                var result = _animationCapabilities.PrepareStopReels(command.ReelStopData);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}
