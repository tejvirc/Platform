namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     Handler for the send token denomination command
    /// </summary>
    public class LPB3SendTokenDenominationHandler : ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LongPollData>
    {
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Create an instance of LPB3SendTokenDenominationHandler
        /// </summary>
        /// <param name="propertiesManager">A reference to the properties manager service</param>
        public LPB3SendTokenDenominationHandler(IPropertiesManager propertiesManager)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll>
        {
            LongPoll.SendTokenDenomination
        };

        /// <inheritdoc/>
        public LongPollReadSingleValueResponse<byte> Handle(LongPollData data)
        {
            var tokenDenominationCents = 0;

            if (_propertiesManager.GetValue(SasProperties.HopperSupportedKey, false) ||
                _propertiesManager.GetValue(SasProperties.CoinAcceptorSupportedKey, false))
            {
                tokenDenominationCents = _propertiesManager.GetValue(SasProperties.TokenDenominationKey, 0);
            }

            return new LongPollReadSingleValueResponse<byte>(DenominationCodes.GetCodeForDenomination(tokenDenominationCents));
        }
    }
}
