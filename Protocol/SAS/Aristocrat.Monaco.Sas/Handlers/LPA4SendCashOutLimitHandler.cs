namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;

    /// <summary>
    ///     The handler for LP A4 Send Cash Out limit
    /// </summary>
    public class LPA4SendCashOutLimitHandler : ISasLongPollHandler<LongPollReadSingleValueResponse<ulong>, SendCashOutLimitData>
    {
        private readonly IGameProvider _gameProvider;
        private readonly IMeterManager _meterManager;

        /// <summary>
        ///     Creates an instance of the LPA4SendCashOutLimitHandler class
        /// </summary>
        /// <param name="meterManager">Required to get the cashout limit meter</param>
        /// <param name="gameProvider">Required for checking if this is the correct game ID</param>
        public LPA4SendCashOutLimitHandler(IMeterManager meterManager, IGameProvider gameProvider)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(IMeterManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(IGameProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendCashOutLimit
        };

        /// <inheritdoc />
        public LongPollReadSingleValueResponse<ulong> Handle(SendCashOutLimitData data)
        {
            var response = new LongPollReadSingleValueResponse<ulong>(0);

            if (data.GameId == 0 || _gameProvider.GetAllGames().Any(game => game.Id == data.GameId))
            {
                response.Data = (ulong)_meterManager.GetMeterValue(data.AccountingDenom, SasMeters.CurrentHopperLevel, MeterType.Lifetime);
            }

            return response;
        }
    }
}
