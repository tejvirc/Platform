namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Metering;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     The LP9A Send Legacy Bonus Meters Handler
    /// </summary>
    public class LP9ASendLegacyBonusMetersHandler :
        ISasLongPollHandler<LongPollSendLegacyBonusMetersResponse, LongPollSendLegacyBonusMetersData>
    {
        private readonly IGameMeterManager _gameMeterManager;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Creates the LP9ASendLegacyBonusMetersHandler instance
        /// </summary>
        /// <param name="gameMeterManager">The meter manager</param>
        /// <param name="gameProvider">The game provider</param>
        public LP9ASendLegacyBonusMetersHandler(IGameMeterManager gameMeterManager, IGameProvider gameProvider)
        {
            _gameMeterManager = gameMeterManager ?? throw new ArgumentNullException(nameof(gameMeterManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendLegacyBonusMeters };

        /// <inheritdoc />
        public LongPollSendLegacyBonusMetersResponse Handle(LongPollSendLegacyBonusMetersData data)
        {
            return new LongPollSendLegacyBonusMetersResponse
            {
                Deductible = GetMeterLifetimeToAccountCredits(
                    SasMeterNames.BonusDeductibleAmount,
                    data.AccountingDenom,
                    data.GameId),
                NonDeductible = GetMeterLifetimeToAccountCredits(
                    SasMeterNames.BonusNonDeductibleAmount,
                    data.AccountingDenom,
                    data.GameId),
                WagerMatch = GetMeterLifetimeToAccountCredits(
                    GamingMeters.WagerMatchBonusAmount,
                    data.AccountingDenom,
                    data.GameId)
            };
        }

        private ulong GetMeterLifetimeToAccountCredits(
            string meterName,
            long accountingDenom,
            int gameId)
        {
	        //GameID 0 means for the entire machine
            if (gameId == 0)
            {
                return MetersExists(meterName, 0, 0)
                    ? (ulong)_gameMeterManager.GetMeter(meterName).Lifetime.MillicentsToAccountCredits(accountingDenom)
                    : 0UL;
            }

            var (game, denom) = _gameProvider.GetGameDetail(gameId);
            if (game is null || denom is null || !MetersExists(meterName, game.Id, denom.Value))
            {
                return 0UL;
            }

            var lifeTime = _gameMeterManager.GetMeter(game.Id, denom.Value, meterName).Lifetime;
            return (ulong)lifeTime.MillicentsToAccountCredits(accountingDenom);
        }

        private bool MetersExists(string meterName, int gameId, long denom)
        {
            return !string.IsNullOrWhiteSpace(meterName)
                   && _gameMeterManager.IsMeterProvided(meterName)
                   && (gameId == 0 || _gameMeterManager.IsMeterProvided(gameId, denom, meterName));
        }
    }
}