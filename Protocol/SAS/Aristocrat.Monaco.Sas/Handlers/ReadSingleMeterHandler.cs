namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     Handles reading a single meter
    /// </summary>
    public class ReadSingleMeterHandler : ISasLongPollHandler<LongPollReadMeterResponse, LongPollReadMeterData>
    {
        private readonly IMeterManager _meterManager;
        private readonly IGameMeterManager _gameMeterManager;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Create an instance of ReadSingleMeterHandler
        /// </summary>
        /// <param name="meterManager">A reference to the meter manager service</param>
        /// <param name="gameMeterManager">The game meter manager</param>
        /// <param name="gameProvider">The game provider</param>
        public ReadSingleMeterHandler(
            IMeterManager meterManager,
            IGameMeterManager gameMeterManager,
            IGameProvider gameProvider)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _gameMeterManager = gameMeterManager ?? throw new ArgumentNullException(nameof(gameMeterManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll>
        {
            LongPoll.SendCanceledCreditsMeter,
            LongPoll.SendCoinInMeter,
            LongPoll.SendCoinOutMeter,
            LongPoll.SendDropMeter,
            LongPoll.SendJackpotMeter,
            LongPoll.SendGamesPlayedMeter,
            LongPoll.SendGamesWonMeter,
            LongPoll.SendGamesLostMeter,
            LongPoll.SendTrueCoinIn,
            LongPoll.SendTrueCoinOut,
            LongPoll.SendCurrentHopperLevel,
            LongPoll.SendOneDollarInMeter,
            LongPoll.SendTwoDollarInMeter,
            LongPoll.SendFiveDollarInMeter,
            LongPoll.SendTenDollarInMeter,
            LongPoll.SendTwentyDollarInMeter,
            LongPoll.SendFiftyDollarInMeter,
            LongPoll.SendOneHundredDollarInMeter,
            LongPoll.SendFiveHundredDollarInMeter,
            LongPoll.SendOneThousandDollarInMeter,
            LongPoll.SendTwoHundredDollarInMeter,
            LongPoll.SendTwentyFiveDollarInMeter,
            LongPoll.SendTwoThousandDollarInMeter,
            LongPoll.SendTwoThousandFiveHundredDollarInMeter,
            LongPoll.SendFiveThousandDollarInMeter,
            LongPoll.SendTenThousandDollarInMeter,
            LongPoll.SendTwentyThousandDollarInMeter,
            LongPoll.SendTwentyFiveThousandDollarInMeter,
            LongPoll.SendFiftyThousandDollarInMeter,
            LongPoll.SendOneHundredThousandDollarInMeter,
            LongPoll.SendTwoHundredFiftyDollarInMeter,
            LongPoll.SendCoinAcceptedFromExternalAcceptor,
            LongPoll.SendNumberOfBillsInStacker,
            LongPoll.SendTotalBillInValueMeter,
            LongPoll.SendCreditAmountOfBillsInStacker
        };

        /// <inheritdoc/>
        public LongPollReadMeterResponse Handle(LongPollReadMeterData data)
        {
            var meterResult = data.TargetDenomination == 0
                ? _meterManager.GetMeterValue(data.AccountingDenom, data.Meter, data.MeterType)
                : _gameMeterManager.GetMeterValue(
                        data.AccountingDenom,
                        data.Meter,
                        data.MeterType,
                        data.TargetDenomination.CentsToMillicents())
                    .CentsToAccountingCredits(data.AccountingDenom);

            if (data.TargetDenomination != 0
                && !_gameProvider.GetAllGames().Any(
                    g => g.SupportedDenominations.Contains(data.TargetDenomination.CentsToMillicents())))
            {
                return new LongPollReadMeterResponse(data.Meter, (ulong)meterResult)
                {
                    ErrorCode = MultiDenomAwareErrorCode.NotValidPlayerDenom
                };
            }
            else
            {
                return new LongPollReadMeterResponse(data.Meter, (ulong)meterResult);
            }
        }
    }
}