namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using BonusProvider;
    using Contracts.Client;
    using Gaming.Contracts.Bonus;

    /// <summary>
    ///     The LP90 Send Legacy Bonus Win Amount Handler
    /// </summary>
    public class LP90SendLegacyBonusWinAmountsHandler : ISasLongPollHandler<LegacyBonusWinAmountResponse, LongPollSingleValueData<long>>
    {
        private readonly ISasBonusCallback _bonusCallback;

        /// <summary>
        ///     Creates the LP90SendLegacyBonusWinAmountsHandler instance
        /// </summary>
        /// <param name="bonusCallback">An instance of <see cref="ISasBonusCallback"/></param>
        public LP90SendLegacyBonusWinAmountsHandler(ISasBonusCallback bonusCallback)
        {
            _bonusCallback = bonusCallback ?? throw new ArgumentNullException(nameof(bonusCallback));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendLegacyBonusWinAmount
        };

        /// <inheritdoc />
        public LegacyBonusWinAmountResponse Handle(LongPollSingleValueData<long> data)
        {
            var response = new LegacyBonusWinAmountResponse
            {
                // TODO: After implementation of LP8B
                MultipliedWin = 0,
                Multiplier = 0,
                Handlers = new HostAcknowledgementHandler()
            };

            var lastPaidLegacyBonus = _bonusCallback.GetLastPaidLegacyBonus();
            if (lastPaidLegacyBonus is null || lastPaidLegacyBonus.State == BonusState.Acknowledged)
            {
                return response;
            }

            response.BonusAmount = (ulong)lastPaidLegacyBonus.PaidAmount.MillicentsToAccountCredits(data.Value);
            response.TaxStatus = lastPaidLegacyBonus.Mode.GeTaxStatus();
            response.Handlers.ImpliedAckHandler = () =>
                Task.Run(() => _bonusCallback.AcknowledgeBonus(lastPaidLegacyBonus.BonusId));
            return response;
        }
    }
}