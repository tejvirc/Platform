namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Monaco.Common;
    using Commands;
    using Kernel;
    using Kernel.Contracts;
    using Services.Lockup;

    /// <summary>
    ///     Handles the <see cref="Reboot"/> message.
    /// </summary>
    public class RebootHandler : MessageHandler<Reboot>
    {
        private readonly IEventBus _eventBus;
        private readonly ILockup _lockup;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly ILogger<RebootHandler> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RebootHandler"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/></param>
        /// <param name="lockup"><see cref="ILockup"/></param>
        /// <param name="commandFactory"><see cref="ICommandHandlerFactory"/></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        public RebootHandler(
            IEventBus eventBus,
            ILockup lockup,
            ICommandHandlerFactory commandFactory,
            ILogger<RebootHandler> logger)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _lockup = lockup ?? throw new ArgumentNullException(nameof(lockup));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override Task<IResponse> Handle(Reboot message)
        {
            if (message.IsClearLocks && _lockup.IsLockedByHost)
            {
                _lockup.ClearHostLock();
            }

            Task.Run(() => _commandFactory.Execute(new UnregisterInstance()))
                .FireAndForget(ex => _logger.LogError(ex.Message));

            _eventBus.Publish(new ExitRequestedEvent(ExitAction.RestartPlatform));

            return Task.FromResult(Ok<RebootResponse>());
        }
    }
}
