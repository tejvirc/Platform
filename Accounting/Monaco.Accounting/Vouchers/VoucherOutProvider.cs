namespace Aristocrat.Monaco.Accounting.Vouchers
{
    using Application.Contracts;
    using Aristocrat.Monaco.Kernel.Contracts.LockManagement;
    using Contracts;
    using Contracts.Transactions;
    using Contracts.TransferOut;
    using Contracts.Vouchers;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An <see cref="ITransferOutProvider" /> for vouchers
    /// </summary>
    [CLSCompliant(false)]
    public class VoucherOutProvider : TransferOutProviderBase, IVoucherOutProvider
    {
        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly IIdProvider _idProvider;
        private readonly IMeterManager _meters;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;
        private readonly IValidationProvider _validationProvider;
        private readonly ILockManager _lockManager;

        public VoucherOutProvider()
            : this(
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IIdProvider>(),
                ServiceManager.GetInstance().GetService<IValidationProvider>(),
                ServiceManager.GetInstance().GetService<ILockManager>())
        {
        }

        public VoucherOutProvider(
            IBank bank,
            ITransactionHistory transactions,
            IMeterManager meters,
            IPersistentStorageManager storage,
            IEventBus bus,
            IPropertiesManager properties,
            IIdProvider idProvider,
            IValidationProvider validationProvider,
            ILockManager lockManager)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _validationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
            _lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));

            // Override MaxSequence if configured
            MaxSequence = _properties.GetValue(AccountingConstants.VoucherOutMaxSequence, MaxSequence);
            Logger.Info($"VoucherOutMaxSequence is set to {MaxSequence}");
        }

        public string Name => typeof(VoucherOutProvider).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IVoucherOutProvider) };

        public bool Active { get; private set; }

        public void Initialize()
        {
        }

        public async Task<TransferResult> Transfer(
            Guid transactionId,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId,
            CancellationToken cancellationToken)
        {
            var validator = _validationProvider.GetVoucherValidator();
            if (validator == null)
            {
                Logger.Info($"No validator - {transactionId}");
                return TransferResult.Failed;
            }
            
            if (!_properties.GetValue(AccountingConstants.VoucherOut, true))
            {
                Logger.Info($"Voucher Out Not Allowed - {transactionId}");
                return TransferResult.Failed;
            }

            if (!CheckVoucherOutLimit(cashableAmount))
            {
                Logger.Info($"Cashable amount exceeds the voucher-out limit - {transactionId}");
                return TransferResult.Failed;
            }

            var transferredCashable = 0L;
            var transferredPromo = 0L;
            var transferredNonCash = 0L;

            try
            {
                Active = true;

                if (validator.CanCombineCashableAmounts)
                {
                    if ((cashableAmount > 0 || promoAmount > 0) &&
                        await Transfer(validator, transactionId, AccountType.Cashable, new VoucherAmount(cashableAmount, promoAmount, 0), associatedTransactions, reason, traceId, cancellationToken))
                    {
                        transferredCashable = cashableAmount;
                        transferredPromo = promoAmount;
                    }
                }
                else
                {
                    if (cashableAmount > 0 && await Transfer(validator, transactionId, AccountType.Cashable, new VoucherAmount(cashableAmount, 0, 0), associatedTransactions, reason, traceId, cancellationToken))
                    {
                        transferredCashable = cashableAmount;
                    }

                    if (promoAmount > 0 && await Transfer(validator, transactionId, AccountType.Promo, new VoucherAmount(0, promoAmount, 0), associatedTransactions, reason, traceId, cancellationToken))
                    {
                        transferredPromo = promoAmount;
                    }
                }

                if (nonCashAmount > 0 && _properties.GetValue(AccountingConstants.VoucherOutNonCash, false) &&
                    await Transfer(validator, transactionId, AccountType.NonCash, new VoucherAmount(0, 0, nonCashAmount), associatedTransactions, reason, traceId, cancellationToken))
                {
                    transferredNonCash = nonCashAmount;
                }

                return new TransferResult(transferredCashable, transferredPromo, transferredNonCash);
            }
            finally
            {
                Active = false;
            }
        }

        public bool CanRecover(Guid transactionId) => false;

        public async Task<bool> Recover(Guid transactionId, CancellationToken cancellationToken)
        {
            // There is nothing to recover for vouchers
            return await Task.FromResult(false);
        }
        private async Task<bool> Transfer(
            IVoucherValidator validator,
            Guid transactionId,
            AccountType accountType,
            VoucherAmount amount,
            IEnumerable<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId,
            CancellationToken token)
        {
            if (!UpdateTransferOutContext(
                context =>
                {
                    context.Barcode = null;
                    return true;
                }))
            {
                return false;
            }

            if (reason.AffectsBalance() && !amount.CheckBankBalance(_bank))
            {
                Logger.Error($"Balance less than amount requested - {transactionId}");
                return await Task.FromResult(false);
            }

            if (token.IsCancellationRequested)
            {
                return await Task.FromResult(false);
            }

            var printer = await GetPrinter(token);
            if (printer == null)
            {
                Logger.Info($"Failed to acquire printer - {transactionId}");
                return await Task.FromResult(false);
            }

            if (!validator.CanValidateVoucherOut(amount.Amount, accountType))
            {
                Logger.Info($"Cannot validate the voucher out - {transactionId}");
                return await Task.FromResult(false);
            }

            _bus.Publish(new VoucherOutStartedEvent(amount.Amount));

            var transaction = await validator.IssueVoucher(amount, accountType, transactionId, reason);
            if (transaction == null)
            {
                Logger.Error($"Failed to issue voucher transaction - {transactionId}");
                return await Task.FromResult(false);
            }

            if (!UpdateTransferOutContext(
                context =>
                {
                    context.Barcode = transaction.Barcode;
                    return true;
                }))
            {
                return false;
            }

            transaction.BankTransactionId = transactionId;
            transaction.AssociatedTransactions = associatedTransactions;
            transaction.TraceId = traceId;
            transaction.Reason = reason;
            transaction.HostOnline = validator.HostOnline;

            return await IssueVoucher(printer, transaction, amount);
        }

        private async Task<bool> IssueVoucher(IPrinter printer, VoucherOutTransaction transaction, VoucherAmount voucherAmount)
        {
            transaction.VoucherSequence = GetTicketSequence(_idProvider.GetCurrentLogSequence<VoucherOutTransaction>() + 1);

            var ticket = VoucherTicketsCreator.GetTicket(transaction);

            if (!await printer.Print(ticket, Commit))
            {
                Logger.Error($"Failed to complete voucher printing: {transaction}");

                if (!transaction.VoucherPrinted && (_validationProvider.GetVoucherValidator()?.ReprintFailedVoucher ?? false))
                {
                    throw new TransferOutException(@"Voucher issuance failed", false);
                }
            }

            return await Task.FromResult(transaction.VoucherPrinted);

            Task Commit()
            {
                Logger.Debug($"Committing the voucher transaction {transaction}");

                try
                {
                    using (var scope = _storage.ScopedTransaction())
                    {
                        using (_lockManager.AcquireExclusiveLock(GetMetersToUpdate()))
                        {
                            if (transaction.Reason.AffectsBalance())
                            {
                                voucherAmount.Withdraw(_bank, transaction.BankTransactionId);
                            }

                            transaction.VoucherPrinted = true;

                            // Unique log sequence number assigned by the EGM; a series that strictly increases by 1 (one) starting at 1 (one).
                            transaction.LogSequence = _idProvider.GetNextLogSequence<VoucherBaseTransaction>();

                            transaction.HostSequence = _idProvider.GetNextLogSequence<IAcknowledgeableTransaction>();

                            // Force the increment, since we just read it before printing
                            _idProvider.GetNextLogSequence<VoucherOutTransaction>();

                            _transactions.AddTransaction(transaction);

                            Logger.Debug("Entering UpdateMeters");

                            UpdateMeters(transaction, voucherAmount);
                            Logger.Debug("Finished UpdateMeters");
                        }

                        scope.Complete();
                    }
                }
                catch (BankException ex)
                {
                    Logger.Fatal($"Failed to debit the bank: {transaction}", ex);

#if !(RETAIL)
                    _bus.Publish(new LegitimacyLockUpEvent());
#endif
                    throw new TransferOutException("Failed to debit the bank: {transaction}", ex);
                }

                Logger.Debug("Preparing to publish VoucherIssuedEvent");

                _bus.Publish(new VoucherIssuedEvent(transaction, ticket));

                Logger.Info($"Voucher issued: {transaction}");

                return Task.CompletedTask;
            }
        }

        private void UpdateMeters(VoucherBaseTransaction transaction, VoucherAmount voucherAmount)
        {
            if (voucherAmount != null && transaction.TypeOfAccount.Equals(AccountType.Cashable) &&
                voucherAmount.PromoAmount > 0)
            {
                var separateMeteringCashableAmounts = _properties.GetValue(AccountingConstants.SeparateMeteringCashableAndPromoOutAmounts, false);

                if (separateMeteringCashableAmounts)
                {
                    _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoAmount).Increment(voucherAmount.PromoAmount);
                    _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoCount).Increment(1);

                    if (voucherAmount.CashAmount > 0)
                    {
                        _meters.GetMeter(AccountingMeters.VoucherOutCashableAmount).Increment(voucherAmount.CashAmount);
                        _meters.GetMeter(AccountingMeters.VoucherOutCashableCount).Increment(1);
                    }
                }
                else
                {
                    _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoAmount).Increment(voucherAmount.PromoAmount);

                    if (voucherAmount.CashAmount > 0)
                    {
                        _meters.GetMeter(AccountingMeters.VoucherOutCashableAmount).Increment(voucherAmount.CashAmount);
                        _meters.GetMeter(AccountingMeters.VoucherOutCashableCount).Increment(1);
                    }
                    else
                    {
                        _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoCount).Increment(1);
                    }
                    
                }
                
            }
            else
            {
                switch (transaction.TypeOfAccount)
                {
                    case AccountType.Cashable:
                        _meters.GetMeter(AccountingMeters.VoucherOutCashableAmount).Increment(transaction.Amount);
                        _meters.GetMeter(AccountingMeters.VoucherOutCashableCount).Increment(1);
                        break;
                    case AccountType.Promo:
                        _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoAmount).Increment(transaction.Amount);
                        _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoCount).Increment(1);
                        break;
                    case AccountType.NonCash:
                        _meters.GetMeter(AccountingMeters.VoucherOutNonCashableAmount).Increment(transaction.Amount);
                        _meters.GetMeter(AccountingMeters.VoucherOutNonCashableCount).Increment(1);
                        break;
                }
            }
        }

        private IEnumerable<IMeter> GetMetersToUpdate()
        {
            return new List<IMeter>()
            {
                _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoAmount),
                _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoCount),

                _meters.GetMeter(AccountingMeters.VoucherOutCashableAmount),
                _meters.GetMeter(AccountingMeters.VoucherOutCashableCount),

                _meters.GetMeter(AccountingMeters.VoucherOutCashableAmount),
                _meters.GetMeter(AccountingMeters.VoucherOutCashableCount),

                _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoAmount),
                _meters.GetMeter(AccountingMeters.VoucherOutCashablePromoCount),

                _meters.GetMeter(AccountingMeters.VoucherOutNonCashableAmount),
                _meters.GetMeter(AccountingMeters.VoucherOutNonCashableCount)
            };
        }

        private bool CheckVoucherOutLimit(long amount)
        {
            var voucherOutLimit = _properties.GetValue(
                AccountingConstants.VoucherOutLimit,
                AccountingConstants.DefaultVoucherOutLimit);

            return amount <= voucherOutLimit;
        }

        private bool UpdateTransferOutContext(Func<ITransferOutContext, bool> update)
        {
            var context = (ITransferOutContext)_properties.GetProperty(AccountingConstants.TransferOutContext, null);

            if (context == null)
            {
                Logger.Error("No transfer out context.");
                return false;
            }

            var success = update(context);
            _properties.SetProperty(AccountingConstants.TransferOutContext, context);

            if (!success)
            {
                Logger.Error("Failed to update transfer out context.");
            }

            return success;
        }
    }
}