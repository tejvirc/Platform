namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="ConnectedReels" /> command.
    /// </summary>
    public class ConnectedReelsCommandHandler : ICommandHandler<ConnectedReels>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IReelController _reelController;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectedReelsCommandHandler" /> class.
        /// </summary>
        public ConnectedReelsCommandHandler()
        {
            _reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
        }

        /// <inheritdoc />
        public void Handle(ConnectedReels command)
        {
            Logger.Debug("Handle ConnectedReels command");

            command.ReelIds = new List<int>(_reelController.ConnectedReels);
        }
    }
}