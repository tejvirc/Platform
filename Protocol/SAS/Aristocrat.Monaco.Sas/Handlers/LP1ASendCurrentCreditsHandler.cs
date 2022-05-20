namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Handles the send current credits command
    /// </summary>
    public class LP1ASendCurrentCreditsHandler : ISasLongPollHandler<LongPollReadMeterResponse, LongPollReadMeterData>
    {
        private readonly IBank _bank;

        /// <summary>
        ///     Create an instance of LP1ASendCurrentCreditsHandler
        /// </summary>
        /// <param name="bank">A reference to the bank</param>
        public LP1ASendCurrentCreditsHandler(IBank bank)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
        }

        /// <inheritdoc/>
        public List<LongPoll> Commands { get; } = new List<LongPoll>
        {
            LongPoll.SendCurrentCredits
        };

        /// <inheritdoc/>
        public LongPollReadMeterResponse Handle(LongPollReadMeterData data)
        {
            var credits = _bank.QueryBalance().MillicentsToAccountCredits(data.AccountingDenom);
            return new LongPollReadMeterResponse(data.Meter, (ulong)credits);
        }
    }
}