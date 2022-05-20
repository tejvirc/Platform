namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Sas;

    /// <summary>
    ///     The LP 52 Send Game N Meters Handler
    /// </summary>
    public class LP52SendGameNMetersHandler : ISasLongPollHandler<LongPollReadMultipleMetersResponse,
        LongPollReadMultipleMetersGameNData>
    {
        private readonly IGameMeterManager _meterManager;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Creates the LP52SendGameNMetersHandler instance
        /// </summary>
        /// <param name="meterManager">The meter manager</param>
        /// <param name="gameProvider">An instance of <see cref="IGameProvider"/></param>
        public LP52SendGameNMetersHandler(IGameMeterManager meterManager, IGameProvider gameProvider)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendGameNMeters };

        /// <inheritdoc />
        public LongPollReadMultipleMetersResponse Handle(LongPollReadMultipleMetersGameNData data)
        {
            var gameId = data.GameNumber;
            var denomValue = 0L;
            if (gameId == 0)
            {
                return new LongPollReadMultipleMetersResponse(
                    GetMeterValues(gameId, denomValue, data.AccountingDenom, data.Meters));
            }

            var (game, denom) = _gameProvider.GetGameDetail(gameId);
            if (game is null || denom is null)
            {
                return new LongPollReadMultipleMetersResponse(
                    data.Meters.ToDictionary(x => x.Meter, x => new LongPollReadMeterResponse(x.Meter, 0L)));
            }

            gameId = game.Id;
            denomValue = denom.Value;

            return new LongPollReadMultipleMetersResponse(
                GetMeterValues(gameId, denomValue, data.AccountingDenom, data.Meters));
        }

        private Dictionary<SasMeters, LongPollReadMeterResponse> GetMeterValues(
            int gameId,
            long denomValue,
            long accountingDenom,
            IEnumerable<LongPollReadMeterData> meters)
        {
            var result = new Dictionary<SasMeters, LongPollReadMeterResponse>();
            foreach (var meter in meters)
            {
                if (gameId > 0)
                {
                    result[meter.Meter] = new LongPollReadMeterResponse(
                        meter.Meter,
                        (ulong)_meterManager.GetMeterValue(
                            accountingDenom,
                            meter.Meter,
                            meter.MeterType,
                            gameId,
                            denomValue));
                }
                else
                {
                    result[meter.Meter] = new LongPollReadMeterResponse(
                        meter.Meter,
                        (ulong)_meterManager.GetMeterValue(accountingDenom, meter.Meter, meter.MeterType));
                }
            }

            return result;
        }
    }
}