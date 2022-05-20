namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="SpinReels" /> command.
    /// </summary>
    public class SpinReelsCommandHandler : ICommandHandler<SpinReels>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IReelController _reelController;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpinReelsCommandHandler" /> class.
        /// </summary>
        public SpinReelsCommandHandler()
        {
            _reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
        }

        /// <inheritdoc />
        public void Handle(SpinReels command)
        {
            Logger.Debug("Handle SpinReels command");

            var result = _reelController.SpinReels(command.SpinData);

            command.Success = result.Result;
        }
    }
}