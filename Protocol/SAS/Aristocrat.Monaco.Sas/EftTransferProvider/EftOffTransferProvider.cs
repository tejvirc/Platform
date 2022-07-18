namespace Aristocrat.Monaco.Sas.EftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Wat;
    using Application.Contracts.Extensions;
    using Contracts.Eft;
    using Contracts.SASProperties;
    using JetBrains.Annotations;
    using Kernel;
    using log4net;

    /// <summary>
    /// Definition of EftOffTransferProvider
    /// </summary>
    public class EftOffTransferProvider : EftTransferProviderBase, IEftOffTransferProvider, IWatTransferOffProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _egmInitiatedCashOutRequestID;

        private long _egmInitiatedCashOutCashableAmount;

        private long _egmInitiatedCashOutPromoteAmount;

        private long _egmInitiatedCashOutNonCashableAmount;

        private readonly IBank _bank;

        private readonly IWatOffProvider _watOffProvider;

        private readonly IEftHostCashOutProvider _hostCashOutProvider;

        /// <summary>
        /// Creates an instance of EftOffTransferProvider
        /// </summary>
        public EftOffTransferProvider(IWatOffProvider watOffProvider,
            ITransactionCoordinator transactionCoordinator,
            IBank bank,
            IPropertiesManager properties,
            IEftHostCashOutProvider hostCashOutProvider) : base(transactionCoordinator, properties)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _watOffProvider = watOffProvider ?? throw new ArgumentNullException(nameof(watOffProvider));
            _hostCashOutProvider = hostCashOutProvider ?? throw new ArgumentNullException(nameof(hostCashOutProvider));
        }

        /// <inheritdoc />
        public bool EftOffRequest(string requestID, AccountType[] accountTypes, ulong amount)
        {
            Logger.Debug("Performing EFT Off request...");

            if (!AllocateEftOffAmount(accountTypes, amount, out var cashableAmount, out var promoteAmount, out var nonCashableAmount))
            {
                return false;
            }

            var handledByHostCashOutProvider = _hostCashOutProvider.CashOutAccepted();
            if (!handledByHostCashOutProvider)
            {
                Logger.Debug("Continue performing Eft Off.");
                var accepted = _watOffProvider.RequestTransfer(
                    requestID,
                    ((long)cashableAmount).CentsToMillicents(),
                    ((long)promoteAmount).CentsToMillicents(),
                    ((long)nonCashableAmount).CentsToMillicents(),
                    PartialTransferAllowed);

                if (!accepted)
                {
                    ReleaseTransactionId();
                }

                Logger.Debug($"End of EftOffRequest -- cash-able={cashableAmount} restricted={nonCashableAmount} non-restricted={promoteAmount} accepted={accepted}");
                return accepted;
            }

            _egmInitiatedCashOutRequestID = requestID;
            _egmInitiatedCashOutCashableAmount = (long)cashableAmount;
            _egmInitiatedCashOutPromoteAmount = (long)promoteAmount;
            _egmInitiatedCashOutNonCashableAmount = (long)nonCashableAmount;
            return true;
        }

        private bool AllocateEftOffAmount(
            AccountType[] accountTypes,
            ulong amount,
            out ulong cashableAmount,
            out ulong promoteAmount,
            out ulong nonCashableAmount)
        {
            if (accountTypes == null)
            {
                throw new ArgumentNullException(nameof(accountTypes));
            }

            cashableAmount = 0;
            promoteAmount = 0;
            nonCashableAmount = 0;

            accountTypes = accountTypes.Distinct().ToArray();
            if (!CanTransfer || amount <= 0 || !accountTypes.Any())
            {
                Logger.Debug("Eft transfer off is not allowed.");
                return false;
            }

            var (acceptedAmount, _) = GetAcceptedAmount(new HashSet<AccountType>(accountTypes), (long)amount);
            if (acceptedAmount == 0)
            {
                Logger.Debug("Eft transfer off: zero accepted amount.");
                return false;
            }

            var allocation = new Dictionary<AccountType, ulong>();
            foreach (var accountType in accountTypes)
            {
                if (acceptedAmount <= 0)
                {
                    break;
                }

                var balance = (ulong)_bank.QueryBalance(accountType).MillicentsToCents();
                var withdraw = Math.Min(acceptedAmount, balance);
                acceptedAmount -= withdraw;
                allocation[accountType] = withdraw;
            }

            ulong GetAllocationByAccountType(AccountType t) => allocation.ContainsKey(t) ? allocation[t] : 0;

            cashableAmount = GetAllocationByAccountType(AccountType.Cashable);
            promoteAmount = GetAllocationByAccountType(AccountType.Promo);
            nonCashableAmount = GetAllocationByAccountType(AccountType.NonCash);
            return true;
        }

        /// <inheritdoc />
        public (ulong Amount, bool LimitExceeded) GetAcceptedTransferOutAmount([NotNull] AccountType[] accountTypes)
        {
            if (accountTypes == null)
            {
                throw new ArgumentNullException(nameof(accountTypes));
            }

            _hostCashOutProvider.RestartTimerIfPendingCallbackFromHost();
            return GetAcceptedAmount(new HashSet<AccountType>(accountTypes), accountTypes.Sum(t => _bank.QueryBalance(t).MillicentsToCents()));
        }

        /// <inheritdoc />
        public void RestartCashoutTimer() => _hostCashOutProvider.RestartTimerIfPendingCallbackFromHost();

        /// <summary>
        /// Gets the type from a service.
        /// </summary>
        /// <returns>The type of the service.</returns>
        public override ICollection<Type> ServiceTypes { get; } = new List<Type> { typeof(IEftOffTransferProvider), typeof(IWatTransferOffProvider) };

        /// <inheritdoc />
        public override bool CanTransfer
        {
            get
            {
                var features = Properties.GetValue(SasProperties.SasFeatureSettings, default(SasFeatures));
                return features?.FundTransferType == FundTransferType.Eft && features.TransferOutAllowed;
            }
        }

        /// <summary>
        ///     Used to initiate a transfer from the the client (EGM)
        /// </summary>
        /// <param name="transaction">A Wat transaction</param>
        /// <returns>true if the transfer was initiated</returns>
        public Task<bool> InitiateTransfer(WatTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (!CanTransfer)
            {
                return Task.FromResult(false);
            }

            if (transaction.Direction == WatDirection.HostInitiated)
            {
                Logger.Debug($"HostInitiated transfer off with cashable={transaction.CashableAmount}, restricted={transaction.PromoAmount}, non-restricted={transaction.NonCashAmount} partial={transaction.AllowReducedAmounts}");
                transaction.PayMethod = WatPayMethod.Credit;
                transaction.UpdateAuthorizedTransactionAmount(_bank, Properties);
                return Task.FromResult(true);
            }

            Logger.Debug("EGM initiating transfer off");

            // this blocking call will return after HOST initiates a long pool request or timeout(800ms).
            var reason = _hostCashOutProvider.HandleHostCashOut();
            if (reason == CashOutReason.None)
            {
                var amountByAccountTypes = new (AccountType AccountType, long Amount)[]
                {
                    (AccountType.Cashable, transaction.CashableAmount.MillicentsToCents()),
                    (AccountType.Promo, transaction.PromoAmount.MillicentsToCents()),
                    (AccountType.NonCash, transaction.NonCashAmount.MillicentsToCents())
                };
                AccountType[] GetAccountTypes() => (from l in amountByAccountTypes where l.Amount != 0 select l.AccountType).ToArray();
                var amount = (ulong)amountByAccountTypes.Sum(l => l.Amount);

                var authorizedCashableAmount = 0L;
                var authorizedPromoAmount = 0L;
                var authorizedNonCashAmount = 0L;
                if (AllocateEftOffAmount(GetAccountTypes(), amount, out var cashableAmount, out var promoAmount, out var nonCashAmount))
                {
                    authorizedCashableAmount = (long)cashableAmount;
                    authorizedPromoAmount = (long)promoAmount;
                    authorizedNonCashAmount = (long)nonCashAmount;
                }

                transaction.UpdateAuthorizedHostCashoutAmount(
                    Properties,
                    PartialTransferAllowed,
                    authorizedCashableAmount,
                    authorizedPromoAmount,
                    authorizedNonCashAmount);
            }
            else if(reason == CashOutReason.CashOutAccepted)
            {
                transaction.RequestId = _egmInitiatedCashOutRequestID;
                transaction.UpdateAuthorizedHostCashoutAmount(
                    Properties,
                    PartialTransferAllowed,
                    _egmInitiatedCashOutCashableAmount,
                     _egmInitiatedCashOutPromoteAmount,
                     _egmInitiatedCashOutNonCashableAmount);
            }

            Logger.Debug("Done with requesting transfer off!");
            return Task.FromResult(reason != CashOutReason.TimedOut);
        }

        /// <summary>
        ///     Used to commit a transfer
        /// </summary>
        /// <param name="transaction">A WAT transaction</param>
        public async Task CommitTransfer(WatTransaction transaction)
        {
            Logger.Debug("Completing transfer off...");
            await Task.Run(EftOffCommitted);
        }

        private (ulong Amount, bool LimitExceeded) GetAcceptedAmount(ISet<AccountType> accountTypes, long amountInCents)
        {
            if (!CanTransfer)
            {
                return (0, true);
            }

            var amount = amountInCents.CentsToMillicents();
            var limitExceeded = false;

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

                    amount = transferLimit;
                    limitExceeded = true;
                }
            }

            // Bank balance checking
            {
                var balance = accountTypes.Sum(t => _bank.QueryBalance(t));
                if (amount > balance)
                {
                    if (!PartialTransferAllowed)
                    {
                        return (0, true);
                    }

                    amount = balance;
                    limitExceeded = true;
                }
            }

            return ((ulong)amount.MillicentsToCents(), limitExceeded);
        }

        private void EftOffCommitted()
        {
            Logger.Debug("EftOffCommitted()");
            ReleaseTransactionId();
        }
    }
}