namespace Aristocrat.Monaco.Sas.EftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Application.Contracts.Extensions;
    using Contracts.Eft;
    using Contracts.SASProperties;
    using Kernel;
    using log4net;

    /// <summary>
    /// Definition of EftOnTransferProvider     
    /// </summary>
    public class EftOnTransferProvider : EftTransferProviderBase, IEftOnTransferProvider, IWatTransferOnProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBank _bank;

        private readonly IWatTransferOnHandler _watOnHandler;

        /// <summary>
        /// Creates an instance of EftOnTransferProvider
        /// </summary>
        public EftOnTransferProvider(IWatTransferOnHandler watTransferOnHandler,
            ITransactionCoordinator transactionCoordinator,
            IBank bank,
            IPropertiesManager properties) : base(transactionCoordinator, properties)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _watOnHandler = watTransferOnHandler ?? throw new ArgumentNullException(nameof(watTransferOnHandler));
        }

        /// <inheritdoc />
        public bool EftOnRequest(string requestID, AccountType accountType, ulong amount)
        {
            Logger.Debug("Start of EftOnRequest().");

            (var acceptedAmount, _) = GetAcceptedTransferInAmount(amount);
            if (!CanTransfer || acceptedAmount == 0)
            {
                Logger.Debug("Eft transfer on is not allowed.");
                return false;
            }

            var cashableAmount = accountType == AccountType.Cashable ? amount : 0;
            var promoteAmount = accountType == AccountType.Promo ? amount : 0;
            var nonCashableAmount = accountType == AccountType.NonCash ? amount : 0;

            var accepted = _watOnHandler.RequestTransfer(
                TransactionId,
                requestID,
                ((long)cashableAmount).CentsToMillicents(),
                ((long)promoteAmount).CentsToMillicents(),
                ((long)nonCashableAmount).CentsToMillicents(),
                PartialTransferAllowed);

            if (!accepted)
            {
                Logger.Error("Failed to perform EftOn. Most likely WatOnHandler is in a wrong state.");
            }

            Logger.Debug($"End of EftOnRequest -- cash-able={cashableAmount} restricted={nonCashableAmount} non-restricted={promoteAmount} accepted={accepted}");

            return accepted;
        }

        /// <inheritdoc />
        public (ulong Amount, bool LimitExceeded) GetAcceptedTransferInAmount(ulong amount)
        {
            var result = GetAcceptedAmount((long)amount);
            return ((ulong)result.Amount, result.LimitExceeded);
        }

        /// <summary>
        ///     Gets the type from a service.
        /// </summary>
        /// <returns>The type of the service.</returns>
        public override ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IEftOnTransferProvider), typeof(IWatTransferOnProvider) };

        /// <inheritdoc />
        public override bool CanTransfer
        {
            get
            {
                var features = Properties.GetValue(SasProperties.SasFeatureSettings, default(SasFeatures));
                return features?.FundTransferType == FundTransferType.Eft && features.TransferInAllowed;
            }
        }

        /// <summary>
        ///     Used to initiate a transfer from the the client (EGM)
        /// </summary>
        /// <param name="transaction">A Wat On transaction</param>
        /// <returns>true if the transfer was initiated</returns>
        public Task<bool> InitiateTransfer(WatOnTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            transaction.UpdateAuthorizedTransactionAmount(_bank, Properties);
            return Task.FromResult(true);
        }

        /// <summary>
        ///     Used to commit a transfer
        /// </summary>
        /// <param name="transaction">A WAT transaction</param>
        public async Task CommitTransfer(WatOnTransaction transaction)
        {
            Logger.Debug("Completing transfer on...");
            await Task.Run(() => EftOnCommitted(transaction));
        }

        private (long Amount, bool LimitExceeded) GetAcceptedAmount(long amountInCents)
        {
            if (!CanTransfer)
            {
                return (0, true);
            }

            var limitExceeded = false;
            var amount = amountInCents.CentsToMillicents();

            // Transfer limit checking
            {
                var settings = Properties.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
                var transferLimit = settings.TransferLimit.CentsToMillicents();
                if (transferLimit > 0 && amount > transferLimit)
                {
                    if (!PartialTransferAllowed)
                    {
                        return (0, true);
                    }

                    limitExceeded = true;
                    amount = transferLimit;
                }
            }

            // Bank limit checking
            {
                if (Properties.GetValue(AccountingConstants.AllowCreditsInAboveMaxCredit, false))
                {
                    return (amount.MillicentsToCents(), limitExceeded);
                }

                var balance = _bank.QueryBalance();
                var total = balance + amount;
                if (!PartialTransferAllowed && total > _bank.Limit)
                {
                    return (0, true);
                }

                if (total > _bank.Limit)
                {
                    limitExceeded = true;
                    amount = _bank.Limit - balance;
                }
            }

            return (amount.MillicentsToCents(), limitExceeded);
        }

        private void EftOnCommitted(WatOnTransaction transaction)
        {
            Logger.Debug("EftOnCommitted()");
            ReleaseTransactionId();
        }
    }
}