namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Kernel;

    /// <summary>
    ///     Handles maintenance mode command
    /// </summary>
    public class MaintenanceModeHandler :
        ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<bool>>
    {
        private readonly ISasDisableProvider _disableProvider;
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Creates a new instance of the MaintenanceModeHandler
        /// </summary>
        /// <param name="disableProvider">The SAS disable provider</param>
        /// <param name="eventBus">The event bus</param>
        public MaintenanceModeHandler(ISasDisableProvider disableProvider, IEventBus eventBus)
        {
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } = new List<LongPoll>()
        {
            LongPoll.EnterMaintenanceMode,
            LongPoll.ExitMaintenanceMode
        };

        /// <inheritdoc />
        public LongPollResponse Handle(LongPollSingleValueData<bool> data)
        {
            if (data.Value)
            {
                _eventBus.Publish(new MaintenanceModeEnteredEvent());
                _disableProvider.Disable(SystemDisablePriority.Normal, DisableState.MaintenanceMode).FireAndForget();
            }
            else
            {
                _eventBus.Publish(new MaintenanceModeExitedEvent());
                _disableProvider.Enable(DisableState.MaintenanceMode).FireAndForget();
            }

            return new LongPollResponse();
        }
    }
}
