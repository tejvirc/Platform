namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.Client;

    /// <summary>
    ///     Handles the startup command
    /// </summary>
    public class LP02StartupHandler : ISasLongPollHandler<LongPollResponse, LongPollSASClientConfigurationData>
    {
        private static readonly DisableState[] Host0EnableStates =
        {
            DisableState.DisabledByHost0, DisableState.PowerUpDisabledByHost0
        };

        private static readonly DisableState[] Host1EnableStates =
        {
            DisableState.DisabledByHost1, DisableState.PowerUpDisabledByHost1
        };

        private readonly ISasDisableProvider _disableProvider;

        /// <summary>
        ///     Creates the LP02StartupHandler instance
        /// </summary>
        /// <param name="disableProvider">The SAS disable provider</param>
        public LP02StartupHandler(ISasDisableProvider disableProvider)
        {
            _disableProvider = disableProvider ?? throw new ArgumentNullException(nameof(disableProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.Startup };

        /// <inheritdoc />
        public LongPollResponse Handle(LongPollSASClientConfigurationData clientConfigurationData)
        {
            var enableStates = clientConfigurationData.ClientConfiguration.ClientNumber == 0
                ? Host0EnableStates
                : Host1EnableStates;
            _disableProvider.Enable(enableStates).FireAndForget();

            return null;
        }
    }
}