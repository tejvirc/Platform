namespace Aristocrat.Monaco.Sas.EftTransferProvider
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client.Eft;
    using Aristocrat.Sas.Client.EFT;
    using Contracts.Eft;
    using log4net;

    /// <summary>
    /// Definition of EftTransferProvider
    /// </summary>
    public class EftTransferProvider : IEftTransferProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEftOffTransferProvider _eftOffTransferProvider;

        private readonly IEftOnTransferProvider _eftOnTransferProvider;

        private readonly ITransactionHistory _transactions;

        private readonly IBank _bank;

        private readonly IMeterManager _meterManager;

        /// <summary>
        /// Creates an instance of EftTransferProvider
        /// </summary>
        public EftTransferProvider(IEftOffTransferProvider eftOffTransferProvider,
            IEftOnTransferProvider eftOnTransferProvider,
            ITransactionHistory transactions,
            IBank bank,
            IMeterManager meterManager)
        {
            _eftOffTransferProvider = eftOffTransferProvider ?? throw new ArgumentNullException(nameof(eftOffTransferProvider));
            _eftOnTransferProvider = eftOnTransferProvider ?? throw new ArgumentNullException(nameof(eftOnTransferProvider));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        /// <inheritdoc />
        public bool DoEftOff(string transactionID, AccountType accountType, ulong amount) => DoEftOff(transactionID, new[] { accountType }, amount);

        /// <inheritdoc />
        public bool DoEftOff(string transactionID, AccountType[] accountTypes, ulong amount) => _eftOffTransferProvider.EftOffRequest(transactionID, accountTypes, amount);

        /// <inheritdoc />
        public bool DoEftOn(string transactionID, AccountType accountType, ulong amount) => _eftOnTransferProvider.EftOnRequest(transactionID, accountType, amount);

        /// <inheritdoc />
        public (ulong Amount, bool LimitExceeded) GetAcceptedTransferInAmount(ulong amount) => _eftOnTransferProvider.GetAcceptedTransferInAmount(amount);

        /// <inheritdoc />
        public (ulong Amount, bool LimitExceeded) GetAcceptedTransferOutAmount(AccountType accountType) => GetAcceptedTransferOutAmount(new[] { accountType });

        /// <inheritdoc />
        public (ulong Amount, bool LimitExceeded) GetAcceptedTransferOutAmount(AccountType[] accountTypes) => _eftOffTransferProvider.GetAcceptedTransferOutAmount(accountTypes);

        /// <inheritdoc />
        public bool CheckIfProcessed(string transactionNumber, EftTransferType transferType)
        {
            if (transferType == EftTransferType.In)
            {
                var lastTransferIn = _transactions.RecallTransactions<WatOnTransaction>()
                    .OrderByDescending(t => t.TransactionDateTime).FirstOrDefault();

                return lastTransferIn is not null && lastTransferIn.RequestId == transactionNumber &&
                       lastTransferIn.Status == WatStatus.Complete;
            }
            else
            {
                var lastTransferOut = _transactions.RecallTransactions<WatTransaction>()
                    .OrderByDescending(t => t.TransactionDateTime).FirstOrDefault();

                return lastTransferOut is not null && lastTransferOut.RequestId == transactionNumber &&
                       lastTransferOut.Status == WatStatus.Complete;
            }
        }

        /// <inheritdoc />
        public CumulativeEftMeterData QueryBalanceAmount()
        {
            //EFT promo credits are non-cash credits of Monaco and
            //EFT non-cash credits are promo credits of Monaco platform
            var result = new CumulativeEftMeterData
            {
                NonCashableCredits = (ulong)_meterManager.GetMeter(AccountingMeters.WatOnCashablePromoAmount).Lifetime,
                CashableCredits = (ulong)_meterManager.GetMeter(AccountingMeters.WatOnCashableAmount).Lifetime,
                PromotionalCredits = (ulong)_meterManager.GetMeter(AccountingMeters.WatOnNonCashableAmount).Lifetime
            };

            var offNonCashAmount = _meterManager.GetMeter(AccountingMeters.WatOffCashablePromoAmount).Lifetime;
            var offCashableAmount = _meterManager.GetMeter(AccountingMeters.WatOffCashableAmount).Lifetime;
            var offPromoAmount = _meterManager.GetMeter(AccountingMeters.WatOffNonCashableAmount).Lifetime;
            result.TransferredCredits = (ulong)(offNonCashAmount + offCashableAmount + offPromoAmount);

            return result;
        }

        /// <inheritdoc />
        public long GetCurrentPromotionalCredits() => _bank.QueryBalance(AccountType.NonCash).MillicentsToCents();

        /// <inheritdoc />
        public (bool SupportEftTransferOn, bool SupportEftTransferOff) GetSupportedTransferTypes() => (_eftOnTransferProvider.CanTransfer, _eftOffTransferProvider.CanTransfer);

        /// <inheritdoc />
        public void RestartCashoutTimer() => _eftOffTransferProvider.RestartCashoutTimer();
    }
}