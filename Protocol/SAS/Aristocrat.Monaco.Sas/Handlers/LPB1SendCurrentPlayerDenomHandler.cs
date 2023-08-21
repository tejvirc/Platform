namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Extensions;
    using Gaming.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     The handler for LP B1 SendPlayer Denomination
    /// </summary>
    public class LPB1SendCurrentPlayerDenomHandler : ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LongPollData>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameHistory _gameHistory;

        /// <summary>
        ///     Creates an instance of the LPB1SendCurrentPlayerDenomHandler class
        /// <param name="propertiesManager">For getting the denomId</param>
        /// <param name="gameHistory">An instance of <see cref="IGameHistory"/></param>
        /// </summary>
        public LPB1SendCurrentPlayerDenomHandler(
            IPropertiesManager propertiesManager,
            IGameHistory gameHistory)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendCurrentPlayerDenominations
        };

        /// <inheritdoc />
        public LongPollReadSingleValueResponse<byte> Handle(LongPollData data)
        {
            if ((bool)_propertiesManager.GetProperty(SasProperties.MultipleDenominationSupportedKey, false))
            {
                var value = (long)_propertiesManager.GetProperty(GamingConstants.SelectedDenom, 0);
                var isGameRunning = (bool)_propertiesManager.GetProperty(GamingConstants.IsGameRunning, false);

                if (!isGameRunning || _gameHistory.IsDiagnosticsActive)
                {
                    value = 0;
                }

                var code = DenominationCodes.GetCodeForDenomination((int)value.MillicentsToCents());
                return new LongPollReadSingleValueResponse<byte>(code);
            }

            return null;
        }
    }
}