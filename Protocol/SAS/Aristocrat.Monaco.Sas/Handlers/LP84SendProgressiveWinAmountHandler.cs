namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Progressive;

    /// <summary>
    ///     The handler for LP 84 Send Progressive Win Amount
    /// </summary>
    public class LP84SendProgressiveWinAmountHandler :
        ISasLongPollHandler<SendProgressiveWinAmountResponse, LongPollData>
    {
        private readonly IProgressiveWinDetailsProvider _progressiveWinDetailsProvider;

        /// <summary>
        ///     Creates a new instance of the LP84SendProgressiveWinAmountHandler
        /// </summary>
        public LP84SendProgressiveWinAmountHandler(IProgressiveWinDetailsProvider progressiveWinDetailsProvider)
        {
            _progressiveWinDetailsProvider = progressiveWinDetailsProvider ?? throw new ArgumentNullException(nameof(progressiveWinDetailsProvider));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll> { LongPoll.SendProgressiveWinAmount };

        /// <inheritdoc />
        public SendProgressiveWinAmountResponse Handle(LongPollData data)
        {
            var response = new SendProgressiveWinAmountResponse();
            var win = _progressiveWinDetailsProvider.GetLastProgressiveWin();
            response.LevelId = win.LevelId;
            response.WinAmount = win.WinAmount.MillicentsToCents();
            response.GroupId = win.GroupId;

            return response;
        }
    }
}