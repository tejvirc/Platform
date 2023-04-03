namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="UpdateReelsSpeed" /> command.
    /// </summary>
    public class UpdateReelsSpeedCommandHandler : ICommandHandler<UpdateReelsSpeed>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelSpinCapabilities _spinCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateReelsSpeedCommandHandler" /> class.
        /// </summary>
        public UpdateReelsSpeedCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController.HasCapability<IReelSpinCapabilities>())
            {
                _spinCapabilities = reelController.GetCapability<IReelSpinCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(UpdateReelsSpeed command)
        {
            Logger.Debug("Handle UpdateReelsSpeed command");

            if (_spinCapabilities is not null)
            {
                var result = _spinCapabilities.SetReelSpeed(command.SpeedData);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}