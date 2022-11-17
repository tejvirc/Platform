namespace Aristocrat.Monaco.Accounting.Handpay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Handpay;
    using Contracts.Tickets;
    using Contracts.Transactions;
    using Contracts.TransferOut;
    using Contracts.Vouchers;
    using Contracts.Wat;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;

    public class HandpayProvider : TransferOutProviderBase, IHandpayProvider, IDisposable
    {
        private const int DeviceId = 1;
        private const int PrintRetryDelay = 10000;
        private readonly INoteAcceptor _noteAcceptor;
        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly IIdProvider _idProvider;
        private readonly IMeterManager _meters;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ISystemDisableManager _systemDisable;
        private readonly ITransactionHistory _transactions;
        private readonly IVoucherOutProvider _voucherOut;
        private readonly IWatOffProvider _wat;
        private readonly IValidationProvider _validationProvider;
        private bool _remoteHandpayMethodSelected;
        private readonly AutoResetEvent _handpayPrintRetry = new AutoResetEvent(false);

        private bool _disposed;

        public HandpayProvider()
            : this(
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IIdProvider>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IVoucherOutProvider>(),
                ServiceManager.GetInstance().GetService<IWatOffProvider>(),
                ServiceManager.GetInstance().GetService<IValidationProvider>(),
                ServiceManager.GetInstance().TryGetService<INoteAcceptor>())
        {
        }

        public HandpayProvider(
            IBank bank,
            ITransactionHistory transactions,
            IMeterManager meters,
            IPersistentStorageManager storage,
            ISystemDisableManager systemDisable,
            IPropertiesManager properties,
            IIdProvider idProvider,
            IEventBus bus,
            IVoucherOutProvider voucherOut,
            IWatOffProvider wat,
            IValidationProvider validationProvider,
            INoteAcceptor noteAcceptor)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _voucherOut = voucherOut ?? throw new ArgumentNullException(nameof(voucherOut));
            _wat = wat ?? throw new ArgumentNullException(nameof(wat));
            _validationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
            _noteAcceptor = noteAcceptor; // NOTE: This one can be null, this is valid.
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(HandpayProvider).ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IHandpayProvider) };

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
            var handpayType = ToHandpayType(reason);

            return await Transfer(
                transactionId,
                cashableAmount,
                promoAmount,
                nonCashAmount,
                reason,
                handpayType,
                associatedTransactions,
                traceId,
                cancellationToken);
        }

        public bool CanRecover(Guid transactionId)
        {
            return _transactions.RecallTransactions<HandpayTransaction>()
                .Any(
                    t => t.BankTransactionId == transactionId &&
                         (t.State == HandpayState.Pending || t.State == HandpayState.Requested ||
                          t.State == HandpayState.Committed && t.PrintTicket && !t.Printed));
        }

        public async Task<bool> Recover(Guid transactionId, CancellationToken cancellationToken)
        {
            if (Active)
            {
                return false;
            }

            var transaction = _transactions.RecallTransactions<HandpayTransaction>()
                .FirstOrDefault(t => t.BankTransactionId == transactionId);

            Logger.Debug($"Checking handpay recovery - {transactionId}");

            if (transaction != null)
            {
                Logger.Info($"Recovering handpay transaction: {transaction}");

                if (transaction.State == HandpayState.Pending || transaction.State == HandpayState.Requested)
                {
                    var voucher = _transactions.RecallTransactions<VoucherOutTransaction>()
                        .OrderByDescending(t => t.TransactionId)
                        .FirstOrDefault(t => t.AssociatedTransactions.Contains(transaction.TransactionId));

                    var wat = _transactions.RecallTransactions<WatTransaction>()
                        .OrderByDescending(t => t.TransactionId)
                        .FirstOrDefault(t => t.AssociatedTransactions.Contains(transaction.TransactionId));

                    if (wat != null || voucher != null)
                    {
                        Logger.Debug($"Recovered completed associated transaction - {wat} {voucher}");
                        using (var scope = _storage.ScopedTransaction())
                        {
                            CommitTransaction(transaction);
                            scope.Complete();
                        }

                        return await Task.FromResult(false);
                    }

                    _bus.Publish(new HandpayStartedEvent(transaction.HandpayType, transaction.CashableAmount, transaction.PromoAmount, transaction.NonCashAmount, transaction.WagerAmount, transaction.EligibleResetToCreditMeter(_properties, _bank)));

                    var keyOff = Initiate(transaction);

                    var validator = _validationProvider.GetHandPayValidator(true);
                    if (validator == null)
                    {
                        Logger.Info($"No validator or validation is currently not allowed - {transactionId}");
                        return false;
                    }

                    validator.ValidateHandpay(
                        transaction.CashableAmount,
                        transaction.PromoAmount,
                        transaction.NonCashAmount,
                        transaction.HandpayType);

                    if (transaction.State == HandpayState.Requested)
                    {
                        await RequestHandpay(validator, transaction);
                    }

                    if (!await HandleKeyOff(validator, transaction, cancellationToken, keyOff))
                    {
                        return false;
                    }

                    transaction.PrintTicket = IsPrintReceiptEnable(transaction);
                    var printer = transaction.PrintTicket ? await GetPrinter(cancellationToken) : null;
                    if (printer == null)
                    {
                        Logger.Error($"Failed to acquire printer: {transaction}");
                    }

                    return await IssueReceipt(printer, transaction);
                }

                if (transaction.State == HandpayState.Committed && transaction.PrintTicket && !transaction.Printed &&
                    _properties.GetValue(AccountingConstants.HandpayReceiptsRequired, false))
                {
                    return await FinishPrintingHandpay(transaction, cancellationToken);
                }
            }

            Logger.Debug($"No handpay recovery needed - {transactionId}");

            return false;
        }

        private async Task<bool> FinishPrintingHandpay(HandpayTransaction transaction, CancellationToken cancellationToken)
        {
            Logger.Debug("FinishPrintingHandpay");

            var printer = await GetPrinter(cancellationToken);
            if (printer == null)
            {
                Logger.Error($"Failed to acquire printer: {transaction}");
            }

            if ((bool)_properties.GetProperty(AccountingConstants.ValidateHandpays, false))
            {
                _systemDisable.Disable(
                    ApplicationConstants.HandpayPendingDisableKey,
                    SystemDisablePriority.Immediate,
                    () => Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.ReceiptPrintingFailed),
                    true,
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReceiptPrintingFailedInfo));
            }

            _bus.Publish(new HandpayKeyedOffEvent(transaction));

            return await IssueReceipt(printer, transaction);
        }

        private async Task<TransferResult> Transfer(
            Guid transactionId,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            TransferOutReason reason,
            HandpayType handpayType,
            IEnumerable<long> associatedTransactions,
            Guid traceId,
            CancellationToken cancellationToken)
        {
            if (!_properties.GetValue(AccountingConstants.RequestNonCash, false))
            {
                Logger.Info($"Non Cash amount of {nonCashAmount} is being removed from the handpay transaction - {transactionId}");

                if (!(handpayType == HandpayType.BonusPay && promoAmount + cashableAmount + nonCashAmount <= 0) && promoAmount + cashableAmount <= 0)
                {
                    Logger.Info($"Handpay resulting amount ends with zero after removing the non cash amount - {transactionId}");
                    return TransferResult.Failed;
                }
                nonCashAmount = 0L;
            }

            var validator = _validationProvider.GetHandPayValidator(true);
            if (validator == null)
            {
                Logger.Info($"No validator or validation is currently not allowed - {transactionId}");
                return TransferResult.Failed;
            }

            if (!validator.ValidateHandpay(cashableAmount, promoAmount, nonCashAmount, handpayType))
            {
                Logger.Info($"Handpay request is not valid - {transactionId}");
                return TransferResult.Failed;
            }

            try
            {
                Active = true;
                var context = (ITransferOutContext)_properties.GetProperty(AccountingConstants.TransferOutContext, null);
                var allowReceipt = _properties.GetValue(AccountingConstants.EnableReceipts, false) &&
                                   (handpayType != HandpayType.GameWin ||
                                    _properties.GetValue(AccountingConstants.AllowGameWinReceipts, false));
                var jurisdictionLargeWinKeyOffType = _properties.GetValue(
                    AccountingConstants.HandpayLargeWinKeyOffStrategy,
                    KeyOffType.LocalHandpay);
                var keyOffType = KeyOffType.LocalHandpay;
                if (handpayType == HandpayType.GameWin)
                {
                    keyOffType = jurisdictionLargeWinKeyOffType;
                }

                var wagerAmount = 0L;
                if (handpayType == HandpayType.GameWin && _properties.GetValue(ApplicationConstants.ShowWagerWithLargeWinInfo, false))
                {
                    wagerAmount = _properties.GetValue(ApplicationConstants.LastWagerWithLargeWinInfo, 0L);
                }

                var transaction = new HandpayTransaction(
                    DeviceId,
                    DateTime.UtcNow,
                    cashableAmount,
                    promoAmount,
                    nonCashAmount,
                    wagerAmount,
                    handpayType,
                    allowReceipt,
                    transactionId)
                {
                    State = HandpayState.Requested,
                    KeyOffType = keyOffType,
                    KeyOffDateTime = DateTime.UtcNow,
                    AssociatedTransactions = associatedTransactions,
                    TraceId = traceId,
                    Barcode = context?.Barcode,
                    HostOnline = validator.HostOnline,
                    Reason = reason
                };

                _transactions.AddTransaction(transaction);

                _bus.Publish(new HandpayStartedEvent(transaction.HandpayType, transaction.CashableAmount, transaction.PromoAmount, transaction.NonCashAmount, transaction.WagerAmount, transaction.EligibleResetToCreditMeter(_properties, _bank)));

                var keyOff = Initiate(transaction);

                await RequestHandpay(validator, transaction);

                if (!await HandleKeyOff(validator, transaction, cancellationToken, keyOff))
                {
                    return TransferResult.Failed;
                }

                transaction.PrintTicket = IsPrintReceiptEnable(transaction);
                var printer = transaction.PrintTicket ? await GetPrinter(cancellationToken) : null;
                if (!await IssueReceipt(printer, transaction))
                {
                    return TransferResult.Failed;
                }

                // NOTE: Handpays may have a keyoff total of zero. This is typically used to award a non-monetary based prize requiring an attendant
                return new TransferResult(transaction.KeyOffCashableAmount, transaction.KeyOffPromoAmount, transaction.KeyOffNonCashAmount, true);
            }
            finally
            {
                Active = false;
            }
        }

        private bool IsPrintReceiptEnable(HandpayTransaction transaction)
        {
            return transaction.PrintTicket &&
                IsPrinterAvailable &&
                transaction.KeyOffType != KeyOffType.LocalCredit &&
                transaction.KeyOffType != KeyOffType.RemoteCredit;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _handpayPrintRetry.Set();
                _handpayPrintRetry.Close();
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private static Ticket GetTicket(HandpayTransaction transaction)
        {
            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin:
                    return HandpayTicketsCreator.CreateGameWinTicket(transaction);
                case HandpayType.CancelCredit:
                    return HandpayTicketsCreator.CreateCanceledCreditsReceiptTicket(transaction);
                case HandpayType.BonusPay:
                    return HandpayTicketsCreator.CreateBonusPayTicket(transaction);
                default:
                    return new Ticket();
            }
        }

        private TaskCompletionSource<KeyOffType> Initiate(HandpayTransaction transaction)
        {
            var keyOff = new TaskCompletionSource<KeyOffType>();

            if (ForceKeyOff(transaction, keyOff))
            {
                return keyOff;
            }

            _bus.Subscribe<RemoteKeyOffEvent>(
                this,
                evt =>
                {
                    // This will allow the host to affect the type when keyed off locally
                    transaction.KeyOffType = evt.KeyOffType;
                    transaction.KeyOffCashableAmount = evt.CashableAmount;
                    transaction.KeyOffPromoAmount = evt.PromoAmount;
                    transaction.KeyOffNonCashAmount = evt.NonCashAmount;

                    if (evt.KeyOffType == KeyOffType.RemoteHandpay || evt.KeyOffType == KeyOffType.RemoteVoucher ||
                        evt.KeyOffType == KeyOffType.RemoteWat || evt.KeyOffType == KeyOffType.RemoteCredit ||
                        evt.KeyOffType == KeyOffType.Cancelled)
                    {
                        keyOff.TrySetResult(transaction.KeyOffType);
                    }

                    _remoteHandpayMethodSelected = evt.SelectedByHost;
                });

            var payResetMethod = _properties.GetValue(AccountingConstants.LargeWinHandpayResetMethod, LargeWinHandpayResetMethod.PayByHand);
            if (payResetMethod == LargeWinHandpayResetMethod.PayByMenuSelection
                && transaction.EligibleResetToCreditMeter(_properties, _bank))
            {
                //as key off method(handpay/credit) would be selected by operator through dialogue menu which would be prompted to operator after toggling jackpot key.
                transaction.KeyOffType = KeyOffType.Unknown;
            }

            _bus.Subscribe<DownEvent>(
                this,
                _ =>
                {
                    if (_properties.GetValue(AccountingConstants.HandpayNoteAcceptorConnectedRequired, false) &&
                        (_noteAcceptor is not { Connected: true } ||
                         _noteAcceptor.ServiceProtocol == ApplicationConstants.Fake && !_noteAcceptor.Enabled))
                    {
                        return;
                    }

                    // Check if we are in another lockup and Keyoff is allowed in this state
                    if (!CanKeyOffWhileInLockUp() && InLockupNotCausedByHandpay())
                    {
                        return;
                    }

                    var allowLocalHandpay = _validationProvider.GetHandPayValidator()?.AllowLocalHandpay ?? false;
                    if (!allowLocalHandpay || transaction.KeyOffType == KeyOffType.Unknown ||
                        _remoteHandpayMethodSelected && payResetMethod == LargeWinHandpayResetMethod.PayByMenuSelection)
                    {
                        Logger.Error($"Button.DownEvent ignored - AllowLocalHandpay={allowLocalHandpay}, KeyOffType={transaction.KeyOffType}, RemoteHandpayMethodSelected={_remoteHandpayMethodSelected}, LargeWinHandpayResetMethod={payResetMethod}");
                        return;
                    }

                    transaction.KeyOffCashableAmount = transaction.CashableAmount;
                    transaction.KeyOffPromoAmount = transaction.PromoAmount;
                    transaction.KeyOffNonCashAmount = transaction.NonCashAmount;

                    keyOff.TrySetResult(transaction.KeyOffType);
                },
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);

            // Only Subscribe to this event when Handpay is of type CancelCredit
            if ((bool)_properties.GetProperty(AccountingConstants.HandpayPendingExitEnabled, false) &&
                transaction.HandpayType == HandpayType.CancelCredit)
            {
                _bus.Subscribe<HandpayPendingCanceledEvent>(
                    this,
                    _ =>
                    {
                        keyOff.TrySetResult(KeyOffType.Cancelled);
                    });
            }

            _systemDisable.Disable(
                ApplicationConstants.HandpayPendingDisableKey,
                SystemDisablePriority.Immediate,
                () => GetMessage(transaction),
                true,
                () => GetHelpTip(transaction));

            return keyOff;
        }

        private async Task RequestHandpay(IHandpayValidator validator, HandpayTransaction transaction)
        {
            await validator.RequestHandpay(transaction);

            // Transition the state to Pending once the validator responds
            transaction.State = HandpayState.Pending;
            _transactions.UpdateTransaction(transaction);

            _bus.Publish(new HandpayAcknowledgedEvent(transaction));
        }

        private async Task<bool> HandleKeyOff(
            IHandpayValidator validator,
            HandpayTransaction transaction,
            CancellationToken cancellationToken,
            TaskCompletionSource<KeyOffType> keyOff)
        {
            _bus.Publish(new HandpayKeyOffPendingEvent(transaction));

            using (cancellationToken.Register(() => { keyOff.TrySetCanceled(); }))
            {
                var keyOffType = await keyOff.Task;

                if (validator.LogTransactionRequired(transaction))
                {
                    transaction.HostSequence = _idProvider.GetNextLogSequence<IAcknowledgeableTransaction>();
                }

                // Set whether or not the host is offline (may have gone offline while waiting for key-off). 
                transaction.HostOnline = validator.HostOnline;

                await Task.Run(() => _bus.UnsubscribeAll(this), cancellationToken);

                if (cancellationToken.IsCancellationRequested || keyOffType == KeyOffType.Cancelled)
                {
                    transaction.KeyOffCashableAmount = 0;
                    transaction.KeyOffPromoAmount = 0;
                    transaction.KeyOffNonCashAmount = 0;
                    transaction.KeyOffType = KeyOffType.Cancelled;
                    transaction.State = HandpayState.Committed;
                    _transactions.UpdateTransaction(transaction);

                    _bus.Publish(new HandpayCanceledEvent(transaction));

                    _systemDisable.Enable(ApplicationConstants.HandpayPendingDisableKey);

                    return await Task.FromResult(false);
                }

                try
                {
                    Logger.Debug($"Committing handpay: {transaction}");

                    transaction.KeyOffType = keyOffType;
                    _transactions.UpdateTransaction(transaction);
                    using var scope = _storage.ScopedTransaction();
                    switch (transaction.KeyOffType)
                    {
                        case KeyOffType.LocalHandpay:
                        case KeyOffType.RemoteHandpay:
                            ToHandpay(transaction);
                            break;
                        case KeyOffType.LocalVoucher:
                        case KeyOffType.RemoteVoucher:
                            if (!await Transfer(_voucherOut, transaction))
                            {
                                throw new TransferOutException(@"Voucher issuance failed", true);
                            }
                            break;
                        case KeyOffType.LocalWat:
                        case KeyOffType.RemoteWat:
                            if (!await Transfer(_wat, transaction))
                            {
                                throw new TransferOutException(@"WAT transfer failed", true);
                            }
                            break;
                        case KeyOffType.LocalCredit:
                        case KeyOffType.RemoteCredit:
                            ToCreditMeter(transaction);
                            break;
                        case KeyOffType.Cancelled:
                            // This should only occur for CancelCredit types and is currently a no-op
                            break;
                        case KeyOffType.Unknown:
                            break;
                    }

                    CommitTransaction(transaction);
                    scope.Complete();
                }
                catch (BankException ex)
                {
                    Logger.Error($"Failed to debit the bank: {transaction}", ex);

#if !(RETAIL)
                    _bus.Publish(new LegitimacyLockUpEvent());
#endif
                    throw new TransferOutException("Failed to debit the bank: {transaction}", ex);
                }
            }

            _systemDisable.Enable(ApplicationConstants.HandpayPendingDisableKey);

            _bus.Publish(new HandpayKeyedOffEvent(transaction));

            return await Task.FromResult(true);
        }

        private void ToHandpay(HandpayTransaction transaction)
        {
            switch (transaction.HandpayType)
            {
                case HandpayType.CancelCredit when transaction.Reason != TransferOutReason.CashWin:
                    if (transaction.KeyOffCashableAmount > 0)
                    {
                        _bank.Withdraw(AccountType.Cashable, transaction.KeyOffCashableAmount, transaction.BankTransactionId);
                    }

                    if (transaction.KeyOffPromoAmount > 0)
                    {
                        _bank.Withdraw(AccountType.Promo, transaction.KeyOffPromoAmount, transaction.BankTransactionId);
                    }

                    if (transaction.KeyOffNonCashAmount > 0)
                    {
                        _bank.Withdraw(AccountType.NonCash, transaction.KeyOffNonCashAmount, transaction.BankTransactionId);
                    }
                    break;
                case HandpayType.GameWin:
                case HandpayType.BonusPay:
                    // These are skipped since they don't actually hit the credit meter
                    break;
            }
        }

        private void ToCreditMeter(HandpayTransaction transaction)
        {
            switch (transaction.HandpayType)
            {
                case HandpayType.CancelCredit:
                    // This has no affect since the money is already on the credit meter
                    break;
                case HandpayType.GameWin:
                case HandpayType.BonusPay:
                    if (transaction.KeyOffCashableAmount > 0)
                    {
                        _bank.Deposit(AccountType.Cashable, transaction.KeyOffCashableAmount, transaction.BankTransactionId);
                    }

                    if (transaction.KeyOffPromoAmount > 0)
                    {
                        _bank.Deposit(AccountType.Promo, transaction.KeyOffPromoAmount, transaction.BankTransactionId);
                    }

                    if (transaction.KeyOffNonCashAmount > 0)
                    {
                        _bank.Deposit(AccountType.NonCash, transaction.KeyOffNonCashAmount, transaction.BankTransactionId);
                    }

                    break;
            }
        }

        private static async Task<bool> Transfer(ITransferOutProvider provider, HandpayTransaction transaction)
        {
            var result = await provider.Transfer(
                transaction.BankTransactionId,
                transaction.KeyOffCashableAmount,
                transaction.KeyOffPromoAmount,
                transaction.KeyOffNonCashAmount,
                new List<long> { transaction.TransactionId },
                transaction.Reason,
                transaction.TraceId,
                CancellationToken.None);

            if (result.Success)
            {
                transaction.KeyOffCashableAmount = result.TransferredCashable;
                transaction.KeyOffPromoAmount = result.TransferredPromo;
                transaction.KeyOffNonCashAmount = result.TransferredNonCash;
            }

            return result.Success;
        }

        private async Task<bool> IssueReceipt(IPrinter printer, HandpayTransaction transaction)
        {
            var handpayTransaction = _transactions.RecallTransactions<HandpayTransaction>()
                .First(t => t.TransactionId == transaction.TransactionId);

            // Give this ticket a sequence number now, which will remain even if we fail to print,
            // and more importantly will be changed if we do later print it again.
            using (var scope = _storage.ScopedTransaction())
            {
                handpayTransaction.ReceiptSequence = GetTicketSequence(_idProvider.GetNextLogSequence<IHandpayTicketCreator>());
                _transactions.UpdateTransaction(handpayTransaction);
                scope.Complete();
            }

            if (printer != null && handpayTransaction.PrintTicket)
            {
                if (!await printer.Print(GetTicket(handpayTransaction), Commit) &&
                    _properties.GetValue(AccountingConstants.HandpayReceiptsRequired, false))
                {
                    Logger.Error($"Failed to print ticket: {handpayTransaction}");

                    if (!printer.Enabled)
                    {
                        _handpayPrintRetry.Reset();
                        _bus.Subscribe<Hardware.Contracts.Printer.EnabledEvent>(this, async (_, token) =>
                        {
                            if (printer.Enabled && await FinishPrintingHandpay(handpayTransaction, token))
                            {
                                Logger.Info($"Handpay completed after printer reconnected: {handpayTransaction}");

                                _bus.Unsubscribe<Hardware.Contracts.Printer.EnabledEvent>(this);

                                _systemDisable.Enable(ApplicationConstants.HandpayPendingDisableKey);

                                _handpayPrintRetry.Set();
                            }
                        });

                        _handpayPrintRetry.WaitOne();
                        return true;
                    }

                    await Task.Delay(PrintRetryDelay);
                    return await FinishPrintingHandpay(handpayTransaction, CancellationToken.None);
                }

                Task Commit()
                {
                    var currentTransaction = _transactions.RecallTransactions<HandpayTransaction>()
                        .First(t => t.TransactionId == transaction.TransactionId);

                    currentTransaction.Printed = true;
                    _transactions.UpdateTransaction(currentTransaction);
                    _bus.Publish(new HandpayReceiptPrintEvent(currentTransaction));

                    return Task.CompletedTask;
                }
            }

            var finishedTransaction = _transactions.RecallTransactions<HandpayTransaction>()
                .First(t => t.TransactionId == transaction.TransactionId);

            Logger.Info($"Handpay completed: {finishedTransaction}");

            _remoteHandpayMethodSelected = false;
            _bus.Publish(new HandpayCompletedEvent(finishedTransaction));

            return true;
        }

        private void CommitTransaction(HandpayTransaction transaction)
        {
            transaction.KeyOffDateTime = DateTime.UtcNow;
            transaction.State = HandpayState.Committed;

            _transactions.UpdateTransaction(transaction);

            UpdateMeters(transaction);
        }

        private void UpdateMeters(HandpayTransaction transaction)
        {
            var total = transaction.KeyOffCashableAmount + transaction.KeyOffPromoAmount + transaction.KeyOffNonCashAmount;

            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin:
                    IncrementGameWinMeters(transaction, total);
                    break;
                case HandpayType.BonusPay:
                    IncrementBonusPayMeters(transaction, total);
                    break;
                case HandpayType.CancelCredit:
                    IncrementCancelCreditMeters(transaction, total);
                    break;
            }

            if (transaction.KeyOffType != KeyOffType.LocalHandpay && transaction.KeyOffType != KeyOffType.RemoteHandpay)
            {
                return;
            }

            if (transaction.KeyOffCashableAmount > 0)
            {
                _meters.GetMeter(AccountingMeters.HandpaidCashableAmount).Increment(transaction.KeyOffCashableAmount);
            }

            if (transaction.KeyOffPromoAmount > 0)
            {
                _meters.GetMeter(AccountingMeters.HandpaidPromoAmount).Increment(transaction.KeyOffPromoAmount);
            }

            if (transaction.KeyOffNonCashAmount > 0)
            {
                _meters.GetMeter(AccountingMeters.HandpaidNonCashableAmount).Increment(transaction.KeyOffNonCashAmount);
            }

            _meters.GetMeter(AccountingMeters.HandpaidOutCount).Increment(1);
        }

        private void IncrementGameWinMeters(HandpayTransaction transaction, long total)
        {
            if (transaction.IsCreditType())
            {
                return;
            }

            if (transaction.PrintTicket)
            {
                if (transaction.Validated)
                {
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedGameWinReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedGameWinReceiptCount).Increment(1);
                }
                else
                {
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedGameWinReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedGameWinReceiptCount).Increment(1);
                }
            }
            else
            {
                if (transaction.Validated)
                {
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedGameWinNoReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedGameWinNoReceiptCount).Increment(1);
                }
                else
                {
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedGameWinNoReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedGameWinNoReceiptCount).Increment(1);
                }
            }
        }

        private void IncrementBonusPayMeters(HandpayTransaction transaction, long total)
        {
            if (transaction.IsCreditType())
            {
                return;
            }

            if (transaction.PrintTicket)
            {
                if (transaction.Validated)
                {
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedBonusPayReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedBonusPayReceiptCount).Increment(1);
                }
                else
                {
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedBonusPayReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedBonusPayReceiptCount).Increment(1);
                }
            }
            else
            {
                if (transaction.Validated)
                {
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedBonusPayNoReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedBonusPayNoReceiptCount).Increment(1);
                }
                else
                {
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedBonusPayNoReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedBonusPayNoReceiptCount).Increment(1);
                }
            }
        }

        private void IncrementCancelCreditMeters(HandpayTransaction transaction, long total)
        {
            if (transaction.PrintTicket)
            {
                if (transaction.Validated)
                {
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedCancelReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedCancelReceiptCount).Increment(1);
                }
                else
                {
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedCancelReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedCancelReceiptCount).Increment(1);
                }
            }
            else
            {
                if (transaction.Validated)
                {
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedCancelNoReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidValidatedCancelNoReceiptCount).Increment(1);
                }
                else
                {
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedCancelNoReceiptAmount).Increment(total);
                    _meters.GetMeter(AccountingMeters.HandpaidNotValidatedCancelNoReceiptCount).Increment(1);
                }
            }
        }

        private static HandpayType ToHandpayType(TransferOutReason reason)
        {
            switch (reason)
            {
                case TransferOutReason.LargeWin:
                    return HandpayType.GameWin;
                case TransferOutReason.BonusPay:
                    return HandpayType.BonusPay;
                default:
                    return HandpayType.CancelCredit;
            }
        }

        private string GetMessage(HandpayTransaction transaction)
        {
            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin:
                case HandpayType.BonusPay:
                    var divisor = _properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
                    var totalAmount = (transaction.CashableAmount + transaction.NonCashAmount + transaction.PromoAmount) / divisor;

                    if (transaction.HandpayType == HandpayType.GameWin && _properties.GetValue(ApplicationConstants.ShowWagerWithLargeWinInfo, false) && transaction.WagerAmount > 0)
                    {
                        var wagerAmount = transaction.WagerAmount / divisor;
                        return Localizer.For(CultureFor.PlayerTicket).FormatString(ResourceKeys.JackpotPendingWithWager,
                            totalAmount.FormattedCurrencyString(), wagerAmount.FormattedCurrencyString());
                    }

                    return Localizer.For(CultureFor.PlayerTicket).FormatString(ResourceKeys.JackpotPending,
                        (transaction.WinAmount() / divisor).FormattedCurrencyString());
            }

            return Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.CancelCreditPending);
        }

        private string GetHelpTip(HandpayTransaction transaction)
        {
            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoHandpayPending);

                default:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoCancelCredit);
            }
        }

        private bool ForceKeyOff(HandpayTransaction transaction, TaskCompletionSource<KeyOffType> keyOff)
        {
            var handpayLargeWinForcedKeyOff = false;
            switch (transaction.HandpayType)
            {
                case HandpayType.GameWin:
                    handpayLargeWinForcedKeyOff = _properties.GetValue(AccountingConstants.HandpayLargeWinForcedKeyOff, false);
                    break;
            }

            if (!handpayLargeWinForcedKeyOff)
            {
                return false;
            }

            transaction.KeyOffCashableAmount = transaction.CashableAmount;
            transaction.KeyOffPromoAmount = transaction.PromoAmount;
            transaction.KeyOffNonCashAmount = transaction.NonCashAmount;

            keyOff.TrySetResult(transaction.KeyOffType);

            return true;
        }

        // Check if we are currently in a lockup disabled state
        private bool InLockupNotCausedByHandpay()
        {
            // Check for any other lockups that are not Handpay or LiveAuthenticationDisable type.
            // These are always present in a handpay lockup
            var currentlyInLockUpThatIsNotHandpay = _systemDisable.CurrentImmediateDisableKeys.Where(
                x =>
                    !x.Equals(ApplicationConstants.HandpayPendingDisableKey)
                    && !x.Equals(ApplicationConstants.LiveAuthenticationDisableKey)).ToList().Count > 0;

            return currentlyInLockUpThatIsNotHandpay;
        }

        private bool CanKeyOffWhileInLockUp() => _properties.GetValue(AccountingConstants.CanKeyOffWhileInLockUp, true);

    }
}