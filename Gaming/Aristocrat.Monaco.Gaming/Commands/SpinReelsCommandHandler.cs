namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="SpinReels" /> command.
    /// </summary>
    public class SpinReelsCommandHandler : ICommandHandler<SpinReels>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelSpinCapabilities _spinCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpinReelsCommandHandler" /> class.
        /// </summary>
        public SpinReelsCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController.HasCapability<IReelSpinCapabilities>())
            {
                _spinCapabilities = reelController.GetCapability<IReelSpinCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(SpinReels command)
        {
            Logger.Debug("Handle SpinReels command");

            if (_spinCapabilities is not null)
            {
                var result = _spinCapabilities.SpinReels(command.SpinData);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}