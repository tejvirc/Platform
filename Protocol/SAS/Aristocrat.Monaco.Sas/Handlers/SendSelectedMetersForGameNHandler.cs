namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Aristocrat.Sas.Client.Metering;
    using Contracts.Metering;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     The handler for Send Selected Meters For Game N
    /// </summary>
    public class SendSelectedMetersForGameNHandler : ISasLongPollMultiDenomAwareHandler<
        SendSelectedMetersForGameNResponse, LongPollSelectedMetersForGameNData>
    {
        private readonly IMeterManager _meterManager;
        private readonly IBank _bank;
        private readonly IGameMeterManager _gameMeterManager;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Creates an Instance of the SendSelectedMetersForGameNHandler
        /// </summary>
        /// <param name="meterManager">The meter manager</param>
        /// <param name="bank">The bank</param>
        /// <param name="gameMeterManager">The game meter manager</param>
        /// <param name="gameProvider">The game provider</param>
        public SendSelectedMetersForGameNHandler(
            IMeterManager meterManager,
            IBank bank,
            IGameMeterManager gameMeterManager,
            IGameProvider gameProvider)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(IMeterManager));
            _bank = bank ?? throw new ArgumentNullException(nameof(IBank));
            _gameMeterManager = gameMeterManager ?? throw new ArgumentNullException(nameof(gameMeterManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendSelectedMetersForGameN,
            LongPoll.SendExtendedMetersForGameN,
            LongPoll.SendExtendedMetersForGameNAlternate
        };

        /// <inheritdoc />
        public SendSelectedMetersForGameNResponse Handle(LongPollSelectedMetersForGameNData data)
        {
            var meterResponse = new List<SelectedMeterForGameNResponse>();
            var denomValue = data.TargetDenomination.CentsToMillicents();
            if (data.TargetDenomination != 0 &&
                !_gameProvider.GetAllGames().Any(g => g.SupportedDenominations.Contains(denomValue)))
            {
                return new SendSelectedMetersForGameNResponse(meterResponse)
                {
                    ErrorCode = MultiDenomAwareErrorCode.NotValidPlayerDenom
                };
            }

            var gameId = data.GameNumber;
            if (gameId != 0)
            {
                if (data.MultiDenomPoll)
                {
                    return new SendSelectedMetersForGameNResponse(meterResponse)
                    {
                        ErrorCode = MultiDenomAwareErrorCode.SpecificDenomNotSupported
                    };
                }

                var (game, denom) = _gameProvider.GetGameDetail((long)gameId);
                if (game is null || denom is null)
                {
                    return new SendSelectedMetersForGameNResponse(new List<SelectedMeterForGameNResponse>());
                }

                gameId = (ulong)game.Id;
                denomValue = denom.Value;
            }

            foreach (var sasMeter in data.RequestedMeters.Select(SasMeterCollection.SasMeterForCode).Where(
                x => x != null && (x.GameMeter || data.GameNumber == 0 || data.TargetDenomination != 0)))
            {
                var meterResult = GetMeterResult(gameId, denomValue, data.AccountingDenom, sasMeter);
                if (meterResult != null)
                {
                    meterResponse.Add(
                        new SelectedMeterForGameNResponse(
                            sasMeter.MeterId,
                            (ulong)meterResult.MeterValue,
                            sasMeter.MeterFieldLength,
                            meterResult.MeterLength));
                }
            }

            return new SendSelectedMetersForGameNResponse(meterResponse);
        }

        private MeterResult GetMeterResult(ulong gameId, long denomValue, long accountingDenom, SasMeter sasMeter)
        {
            switch (sasMeter.MeterId)
            {
                case SasMeterId.CurrentCredits:
                    return (gameId == 0 && denomValue == 0)
                        ? new MeterResult(
                            _bank.QueryBalance().MillicentsToAccountCredits(accountingDenom),
                            SasConstants.MaxMeterLength)
                        : null;
                case SasMeterId.CurrentRestrictedCredits:
                    return (gameId == 0 && denomValue == 0)
                        ? new MeterResult(
                            _bank.QueryBalance(AccountType.NonCash).MillicentsToAccountCredits(accountingDenom),
                            SasConstants.MaxMeterLength)
                        : null;
                default:
                    if (gameId != 0)
                    {
                        return (denomValue != 0)
                            ? _gameMeterManager.GetMeterValue(accountingDenom, sasMeter, denomValue, (int)gameId)
                            : _gameMeterManager.GetMeterValue(accountingDenom, sasMeter, (int)gameId);
                    }
                    else if (denomValue != 0)
                    {
                        return _gameMeterManager.GetMeterValue(accountingDenom, sasMeter, denomValue);
                    }

                    return _meterManager.GetMeterValue(accountingDenom, sasMeter);
            }
        }
    }
}