namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Aristocrat.Sas.Client.Metering;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     Handles sending cumulative progressive win
    /// </summary>
    public class LP83SendCumulativeProgressiveWinHandler :
        ISasLongPollHandler<SendCumulativeProgressiveWinResponse, SendCumulativeProgressiveWinData>
    {
        private const int AllGame = 0;
        private static readonly IReadOnlyList<SasMeterId> ProgressiveWinMeters = new List<SasMeterId>
        {
            SasMeterId.TotalMachinePaidProgressiveWin,
            SasMeterId.TotalAttendantPaidProgressiveWin
        };

        private readonly IGameMeterManager _meterManager;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LP83SendCumulativeProgressiveWinHandler" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="meterManager">An instance of <see cref="IGameMeterManager"/></param>
        /// <param name="gameProvider">The game provider</param>
        public LP83SendCumulativeProgressiveWinHandler(
            IGameMeterManager meterManager,
            IGameProvider gameProvider)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.SendCumulativeProgressiveWins };

        /// <inheritdoc />
        public SendCumulativeProgressiveWinResponse Handle(SendCumulativeProgressiveWinData data)
        {
            var gameId = AllGame;
            var denomId = 0L;
            if (data.GameId != AllGame)
            {
                var (game, denom) = _gameProvider.GetGameDetail(data.GameId);
                if (game is null || denom is null)
                {
                    return new SendCumulativeProgressiveWinResponse();
                }

                gameId = game.Id;
                denomId = denom.Value;
            }

            var response = new SendCumulativeProgressiveWinResponse
            {
                MeterValue = (ulong)ProgressiveWinMeters
                    .Select(x => GetMeterValue(x, data.AccountingDenom, gameId, denomId)).Where(x => x != null)
                    .Sum(x => x.MeterValue)
            };

            return response;
        }

        private MeterResult GetMeterValue(SasMeterId meterId, long accountingDenom, int gameId, long denomId)
        {
            var sasMeter = SasMeterCollection.SasMeterForCode(meterId);
            if (sasMeter is null)
            {
                return null;
            }

            if (gameId == AllGame)
            {
                return _meterManager.GetMeterValue(accountingDenom, sasMeter);
            }

            return denomId == 0
                ? _meterManager.GetMeterValue(accountingDenom, sasMeter, gameId)
                : _meterManager.GetMeterValue(accountingDenom, sasMeter, denomId, gameId);
        }
    }
}