namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Reflection;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.Reel.Capabilities;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="PrepareSynchronizeReels" /> command.
    /// </summary>
    public class PrepareSynchronizeCommandHandler : ICommandHandler<PrepareSynchronizeReels>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IReelSynchronizationCapabilities _synchronizationCapabilities;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrepareSynchronizeCommandHandler" /> class.
        /// </summary>
        public PrepareSynchronizeCommandHandler()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

            if (reelController?.HasCapability<IReelSynchronizationCapabilities>() ?? false)
            {
                _synchronizationCapabilities = reelController.GetCapability<IReelSynchronizationCapabilities>();
            }
        }

        /// <inheritdoc />
        public void Handle(PrepareSynchronizeReels command)
        {
            Logger.Debug("Handle PrepareSynchronizeReels command");

            if (_synchronizationCapabilities is not null)
            {
                var result = _synchronizationCapabilities.Synchronize(command.ReelSyncData, default);
                command.Success = result.Result;
                return;
            }

            command.Success = false;
        }
    }
}
