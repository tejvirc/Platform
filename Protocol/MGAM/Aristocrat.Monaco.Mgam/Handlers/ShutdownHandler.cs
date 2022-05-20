namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Kernel;
    using Kernel.Contracts;
    using Services.Lockup;
    using Services.PlayerTracking;

    /// <summary>
    ///     Handles the <see cref="Shutdown"/> message.
    /// </summary>
    public class ShutdownHandler : MessageHandler<Shutdown>
    {
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IPlayerTracking _playerTracking;
        private readonly ILockup _lockup;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShutdownHandler"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="eventBus"><see cref="IEventBus"/></param>
        /// <param name="playerTracking"><see cref="IPlayerTracking"/></param>
        /// <param name="lockup"><see cref="ILockup"/></param>
        public ShutdownHandler(ILogger<ShutdownHandler> logger, IEventBus eventBus, IPlayerTracking playerTracking, ILockup lockup)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _playerTracking = playerTracking ?? throw new ArgumentNullException(nameof(playerTracking));
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
        }

        /// <inheritdoc />
        public override Task<IResponse> Handle(Shutdown message)
        {
            if (!_playerTracking.IsSessionActive && _lockup.IsLockedByHost)
            {
                _eventBus.Publish(new ExitRequestedEvent(ExitAction.ShutDown));
            }
            else
            {
                _lockup.LockupForEmployeeCard();
                _logger.LogInfo("Attempted Shutdown ignored until player is logged out and system is locked by host.");
            }

            return Task.FromResult(Ok<ShutdownResponse>());
        }
    }
}
