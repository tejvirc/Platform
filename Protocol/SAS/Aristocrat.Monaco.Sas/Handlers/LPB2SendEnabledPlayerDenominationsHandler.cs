namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     The handler for LPB2SendEnabledPlayerDenominations
    /// </summary>
    public class LPB2SendEnabledPlayerDenominationsHandler : ISasLongPollHandler<LongPollEnabledPlayerDenominationsResponse, LongPollData>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Creates an instance of the LPB2SendEnabledPlayerDenominations class
        /// <param name="propertiesManager">For getting if multi denomination available</param>
        /// <param name="gameProvider">For getting the correct game.</param>
        /// </summary>
        public LPB2SendEnabledPlayerDenominationsHandler(IPropertiesManager propertiesManager, IGameProvider gameProvider)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(IPropertiesManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(IGameProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendEnabledPlayerDenominations
        };

        /// <inheritdoc />
        public LongPollEnabledPlayerDenominationsResponse Handle(LongPollData data)
        {
            LongPollEnabledPlayerDenominationsResponse result = null;
            if (_propertiesManager.GetValue(SasProperties.MultipleDenominationSupportedKey, false))
            {
                result = new LongPollEnabledPlayerDenominationsResponse(
                    _gameProvider.GetEnabledGames().SelectMany(game => game.GetCodesFromDenominations()).Distinct().ToList());
            }

            return result;
        }
    }
}
