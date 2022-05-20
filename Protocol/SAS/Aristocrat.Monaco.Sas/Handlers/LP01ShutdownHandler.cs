namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;
    using Kernel;

    /// <summary>
    ///     Handles the shut down command
    /// </summary>
    public class LP01ShutdownHandler : ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<byte>>
    {
        private readonly ISasDisableProvider _disableProvider;

        /// <summary>
        ///     Creates the LP01ShutdownHandler instance
        /// </summary>
        /// <param name="disableProvider">The SAS disable provider</param>
        public LP01ShutdownHandler(ISasDisableProvider disableProvider)
        {
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.Shutdown };

        /// <inheritdoc/>
        public LongPollResponse Handle(LongPollSingleValueData<byte> data)
        {
            var state = data.Value == 0
                ? DisableState.DisabledByHost0
                : DisableState.DisabledByHost1;
            _disableProvider.Disable(SystemDisablePriority.Normal, state).FireAndForget();
            return null;
        }
    }
}