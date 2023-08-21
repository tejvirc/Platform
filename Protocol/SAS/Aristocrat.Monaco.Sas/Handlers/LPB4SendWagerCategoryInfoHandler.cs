namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Aristocrat.Sas.Client.Metering;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     The handler for LP B4 Send Wager Category Info
    /// </summary>
    public class LPB4SendWagerCategoryInfoHandler : ISasLongPollHandler<LongPollSendWagerResponse, LongPollReadWagerData>
    {
        private readonly IGameProvider _gameProvider;
        private readonly IGameMeterManager _gameMeterManager;

        /// <summary>
        ///     Creates an instance of the LPB4SendWagerCategoryInfoHandler class
        ///     If percent ret value is 0 then this wager percent is not available.
        /// </summary>
        /// <param name="gameMeterManager">For getting the coin in per wager category</param>
        /// <param name="gameProvider">For getting the correct game.</param>
        public LPB4SendWagerCategoryInfoHandler(
            IGameMeterManager gameMeterManager,
            IGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(IGameProvider));
            _gameMeterManager = gameMeterManager ?? throw new ArgumentNullException(nameof(IGameMeterManager));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendWagerCategoryInformation };

        /// <inheritdoc />
        public LongPollSendWagerResponse Handle(LongPollReadWagerData data)
        {
            // if there is a game id and wager category then look them up.
            // error out if a game or wager category is not found
            // else mark valid and return the data
            if (data.GameId != 0 && data.WagerCategory != 0)
            {
                return HandleNonZeroIdAndWageCategory(data);
            }

            // if game id not 0 and wager ID is zero then get the overall wager amount for the game
            if (data.GameId != 0 && data.WagerCategory == 0)
            {
                return HandleZeroWageCategory(data);
            }

            // if game ID is zero but there is a wager cat then return all zeros and mark valid
            if (data.GameId == 0 && data.WagerCategory != 0)
            {
                return HandleZeroGameId();
            }

            // if both game id and wager cat are zero then return the TrueCoinIn meter data.
            return HandleZeroGameIdAndZeroWagerCategory(data);
        }

        private static LongPollSendWagerResponse HandleZeroGameId()
        {
            var response = new LongPollSendWagerResponse(0, 0, 0, true);
            return response;
        }

        private static int GetMeterLength(long meterRollOver, int meterLength = 0)
        {
            const long digitDivisor = 10;
            return meterRollOver == 0 ? meterLength : GetMeterLength(meterRollOver / digitDivisor, ++meterLength);
        }

        private LongPollSendWagerResponse HandleNonZeroIdAndWageCategory(LongPollReadWagerData data)
        {
            var response = new LongPollSendWagerResponse();
            var (game, denom) = _gameProvider.GetGameDetail(data.GameId);
            int index = data.WagerCategory - 1;
            var wagerCategory = index >= game?.WagerCategories?.Count() || index < 0 ? null : game?.WagerCategories?.ElementAt(index);

            if (denom is null || wagerCategory is null ||
                !_gameMeterManager.IsMeterProvided(
                    game.Id,
                    denom.Value,
                    wagerCategory.Id,
                    GamingMeters.WagerCategoryWageredAmount))
            {
                return response;
            }

            var coinInMeter = _gameMeterManager.GetMeter(
                game.Id,
                denom.Value,
                wagerCategory.Id,
                GamingMeters.WagerCategoryWageredAmount);
            response.CoinInMeter = (int)coinInMeter.Lifetime.MillicentsToCents();
            response.CoinInMeterLength =
                GetMeterLength(coinInMeter.Classification.UpperBounds.MillicentsToCents() - 1);
            response.PaybackPercentage = (int)wagerCategory.TheoPaybackPercent.ToMeter();
            response.IsValid = true;
            return response;
        }

        private LongPollSendWagerResponse HandleZeroWageCategory(LongPollReadWagerData data)
        {
            var response = new LongPollSendWagerResponse();
            var (game, denom) = _gameProvider.GetGameDetail(data.GameId);
            var wagerCategory =
                game?.WagerCategories?.FirstOrDefault(w => w.MaxWagerCredits == game.MaximumWagerCredits);

            if (denom is null || wagerCategory is null ||
                !_gameMeterManager.IsMeterProvided(game.Id, denom.Value, GamingMeters.WageredAmount))
            {
                return response;
            }

            var coinInMeter = _gameMeterManager.GetMeter(game.Id, denom.Value, GamingMeters.WageredAmount);
            response.CoinInMeter = (int)coinInMeter.Lifetime.MillicentsToCents();
            response.CoinInMeterLength = GetMeterLength(coinInMeter.Classification.UpperBounds.MillicentsToCents() - 1);
            response.PaybackPercentage = (int)wagerCategory.TheoPaybackPercent.ToMeter();
            response.IsValid = true;

            return response;
        }

        private LongPollSendWagerResponse HandleZeroGameIdAndZeroWagerCategory(LongPollReadWagerData data)
        {
            var response = new LongPollSendWagerResponse();
            var sasMeter = SasMeterCollection.SasMeterForCode(SasMeterId.TotalCoinIn);
            var meterResult = _gameMeterManager.GetMeterValue(data.AccountingDenom, sasMeter);
            if (meterResult != null)
            {
                long length = meterResult.MeterLength;
                response.CoinInMeter = (int)meterResult.MeterValue;
                response.CoinInMeterLength = (int)length;
                response.IsValid = true;
            }

            return response;
        }
    }
}