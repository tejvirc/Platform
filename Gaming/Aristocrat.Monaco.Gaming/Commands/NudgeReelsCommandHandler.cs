namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="NudgeReels" /> command.
    /// </summary>
    public class NudgeReelsCommandHandler : ICommandHandler<NudgeReels>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IReelController _reelController;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NudgeReelsCommandHandler" /> class.
        /// </summary>
        public NudgeReelsCommandHandler()
        {
            _reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
        }

        /// <inheritdoc />
        public void Handle(NudgeReels command)
        {
            Logger.Debug("Handle NudgeReels command");

            var result = _reelController.NudgeReel(command.NudgeSpinData);

            command.Success = result.Result;
        }
    }
}