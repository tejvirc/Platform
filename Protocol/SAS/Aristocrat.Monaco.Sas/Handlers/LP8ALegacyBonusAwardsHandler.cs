namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     The LP8A Legacy Bonus Award Handler
    /// </summary>
    public class LP8ALegacyBonusAwardsHandler : ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>
    {
        private const byte RespondIgnore = 0;
        private const byte RespondAck = 1;
        private const byte RespondBusy = 2;
        private readonly ISasBonusCallback _bonus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISystemDisableManager _disableManager;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IFundsTransferDisable _fundsTransferDisable;

        /// <summary>
        ///     Creates the LP8ALegacyBonusAwardsHandler instance
        /// </summary>
        /// <param name="bonus">The bonus provider</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="disableManager">The disable manager</param>
        /// <param name="transactionHistory">The transaction history manager</param>
        /// <param name="fundsTransferDisable">The fund transfer disable interface</param>
        public LP8ALegacyBonusAwardsHandler(
            ISasBonusCallback bonus,
            IPropertiesManager propertiesManager,
            ISystemDisableManager disableManager,
            ITransactionHistory transactionHistory,
            IFundsTransferDisable fundsTransferDisable)
        {
            _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _fundsTransferDisable = fundsTransferDisable ?? throw new ArgumentNullException(nameof(fundsTransferDisable));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.InitiateLegacyBonusPay
        };

        /// <inheritdoc />
        public LongPollReadSingleValueResponse<byte> Handle(LegacyBonusAwardsData data)
        {
            var response = new LongPollReadSingleValueResponse<byte>(RespondIgnore);
            var legacyBonusingEnabled = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                .LegacyBonusAllowed;

            if (legacyBonusingEnabled)
            {
                var transaction = _transactionHistory.RecallTransactions<HandpayTransaction>()
                    .OrderBy(x => x.TransactionDateTime)
                    .FirstOrDefault(x => x.State == HandpayState.Pending || x.State == HandpayState.Requested);

                // System not disabled OR HandpayTransaction is pending and it is the Only system disable reason
                // TODO: Add System Validation check too, when implemented in future.
                var disableIsOnlyHandpay = transaction != null;
                if (disableIsOnlyHandpay)
                {
                    // Live Authentication Disable starts at the same time as handpay disable, so ignore it if there is a handpay.
                    disableIsOnlyHandpay = _disableManager.CurrentDisableKeys.All(
                        x => (x.Equals(ApplicationConstants.HandpayPendingDisableKey) ||
                              x.Equals(ApplicationConstants.LiveAuthenticationDisableKey)));
                }

                var isGameRunning = (bool)_propertiesManager.GetProperty(GamingConstants.IsGameRunning, false);

                if ((!_disableManager.IsDisabled || disableIsOnlyHandpay) && isGameRunning
                    && !_fundsTransferDisable.TransferOnDisabledOverlay)
                {
                    Task.Run(() => _bonus.AwardLegacyBonus(data.BonusAmount.AccountingCreditsToCents(data.AccountingDenom), data.TaxStatus));
                    response.Data = RespondAck;
                }
                else
                {
                    response.Data = RespondBusy;
                }
            }

            return response;
        }
    }
}