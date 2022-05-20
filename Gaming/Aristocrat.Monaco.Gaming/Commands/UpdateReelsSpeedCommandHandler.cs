namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="UpdateReelsSpeed" /> command.
    /// </summary>
    public class UpdateReelsSpeedCommandHandler : ICommandHandler<UpdateReelsSpeed>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IReelController _reelController;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateReelsSpeedCommandHandler" /> class.
        /// </summary>
        public UpdateReelsSpeedCommandHandler()
        {
            _reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
        }

        /// <inheritdoc />
        public void Handle(UpdateReelsSpeed command)
        {
            Logger.Debug("Handle UpdateReelsSpeed command");

            var result = _reelController.SetReelSpeed(command.SpeedData);

            command.Success = result.Result;
        }
    }
}