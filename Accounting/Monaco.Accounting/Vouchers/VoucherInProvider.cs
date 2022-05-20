namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Kernel.Contracts.LockManagement;
    using Contracts;
    using Contracts.Vouchers;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using log4net;

    public class VoucherInProvider : TransferInProviderBase, IService, IDisposable
    {
        private const int BarcodeLength = 18;
        private const int RequestTimeoutLength = 1000; // It's in milliseconds

        private static readonly ILog Logger = LogManager.GetLogger(nameof(VoucherInProvider));
        private static readonly Guid RequestorId = new Guid("{B38E80F0-1B82-4571-AEE6-75ADEB9A13BB}");

        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly ITransactionCoordinator _coordinator;
        private readonly IMeterManager _meters;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;
        private readonly IIdProvider _idProvider;
        private readonly IMessageDisplay _messageDisplay;
        private readonly IValidationProvider _validationProvider;
        private readonly ILockManager _lockManager;

        private readonly Dictionary<VoucherInExceptionCode, DisplayableMessage> _exceptionDisplayMessageMap =
            new Dictionary<VoucherInExceptionCode, DisplayableMessage>
            {
                {
                    VoucherInExceptionCode.ZeroAmount, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.ZeroAmount),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{590926F2-C150-4BC3-85EC-BD53D234271A}"))
                },
                {
                    VoucherInExceptionCode.CreditLimitExceeded, new DisplayableMessage(
                        () => GetCreditLimitExceededRejectionMessage(ResourceKeys.CreditLimitExceeded),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{CB91FE64-78F6-4DCB-861C-BFAA601CD02C}"))
                },
                {
                    VoucherInExceptionCode.CreditInLimitExceeded, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.CreditInLimitExceeded),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{15FFA75D-9A3A-4061-A533-8DCD6A153F91}"))
                },
                {
                    VoucherInExceptionCode.LaundryLimitExceeded, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.SessionLimitReached),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{7FC12799-9F68-4387-AF3F-33428A1843C7}"))
                },
                {
                    VoucherInExceptionCode.VoucherInLimitExceeded, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.VoucherLimitExceeded),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{4EE90A8E-EB01-476D-9E56-C3285A84D1D5}"))
                },
                {
                    VoucherInExceptionCode.ValidationFailed, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.ValidationFailed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{C12A99D3-06F1-499E-93DF-697C77FEF5B2}"))
                },
                {
                    VoucherInExceptionCode.InvalidTicket, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.InvalidVoucher),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{BB84BAEA-A6C0-4961-9D36-B3FCC6D01BE7}"))
                },
                {
                    VoucherInExceptionCode.AlreadyReedemed, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.AlreadyRedeemed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{9AC71513-A483-45CE-A2F2-CA0A0A1C4DDC}"))
                },
                {
                    VoucherInExceptionCode.Expired, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.Expired),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{C9CD6C53-01AA-441A-8564-0BCC6DC009E2}"))
                },
                {
                    VoucherInExceptionCode.PlayerCardMustBeInserted, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.PlayerCardMustBeInserted),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{8AC627DC-26F7-48DA-801A-D836F764B2B9}"))
                },
                {
                    VoucherInExceptionCode.TimedOut, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.TimedOut),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{E2D4D171-CB68-453E-A061-41993E72343E}"))
                },
                {
                    VoucherInExceptionCode.InProcessAtAnotherLocation, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.ValidationFailed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{B9651E70-7F17-467E-B63F-87D18EE72A47}"))
                },
                {
                    VoucherInExceptionCode.IncorrectPlayer, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.IncorrectPlayer),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{BBE848F6-5D09-4D55-8332-12D870F91C40}"))
                },
                {
                    VoucherInExceptionCode.PrinterError, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.PrinterError),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{713D8DA0-63C8-4484-BCCA-E797D5B532AE}"))
                },
                {
                    VoucherInExceptionCode.AnotherTransferInProgress, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.ValidationFailed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{7CB4A8EF-1CE7-474C-A31F-0F1839F8046A}"))
                },
                {
                    VoucherInExceptionCode.CannotMixNonCashableExpired, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.ValidationFailed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{4DFD99AE-442F-40D4-95A6-56E4DEB86CE8}"))
                },
                {
                    VoucherInExceptionCode.CannotMixNonCashableCredits, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.ValidationFailed),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{F4E88C86-F647-486C-9BE8-F30B82B92AFE}"))
                },
                {
                    VoucherInExceptionCode.Other, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.NoteAcceptorFailure),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{6DE508FC-E465-4281-8884-BE03DB0D8826}"))
                },
                {
                    VoucherInExceptionCode.PowerFailure, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.PowerFailure),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{D019E801-F3D7-4384-9C33-C06AE245204E}"))
                },
                {
                    VoucherInExceptionCode.NoteAcceptorFailure, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.NoteAcceptorFailure),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{1CB15220-F511-4381-B2A9-5CB98A8349A6}"))
                },
                {
                    VoucherInExceptionCode.TicketingDisabled, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.TicketingDisabled),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRejectedEvent),
                        new Guid("{EB771858-1ECC-483B-B40F-88D6E4363911}"))
                }
            };

        private INoteAcceptor _noteAcceptor;
        private bool _disposed;

        public VoucherInProvider()
            : this(
                ServiceManager.GetInstance().TryGetService<INoteAcceptor>(),
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionCoordinator>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IIdProvider>(),
                ServiceManager.GetInstance().GetService<IMessageDisplay>(),
                ServiceManager.GetInstance().GetService<IValidationProvider>(),
                ServiceManager.GetInstance().GetService<ILockManager>())
        {
        }

        public VoucherInProvider(
            INoteAcceptor noteAcceptor,
            IBank bank,
            ITransactionCoordinator coordinator,
            ITransactionHistory transactionHistory,
            IEventBus bus,
            IMeterManager meters,
            IPropertiesManager properties,
            IPersistentStorageManager storage,
            IIdProvider idProvider,
            IMessageDisplay messageDisplay,
            IValidationProvider validationProvider,
            ILockManager lockManager
            )
            : base(bank, meters, properties)
        {
            _noteAcceptor = noteAcceptor;
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _validationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
            _lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
        }

        private bool TimedOutByHost => _properties.GetValue(AccountingConstants.IsVoucherRedemptionTimedOut, false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name { get; } = typeof(VoucherInProvider).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(VoucherInProvider) };

        public void Initialize()
        {
            _bus.Subscribe<ServiceAddedEvent>(this, Handle);
            _bus.Subscribe<VoucherEscrowedEvent>(this, Handle);
            _bus.Subscribe<VoucherReturnedEvent>(this, Handle);
            _bus.Subscribe<DocumentRejectedEvent>(this, Handle);
            _bus.Subscribe<ProtocolLoadedEvent>(this, evt => Recover());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        protected override void Recover()
        {
            var transaction = _transactions.RecallTransactions<VoucherInTransaction>()
                .FirstOrDefault(t => t.State == VoucherState.Pending);
            if (transaction == null)
            {
                return;
            }

            Guid transactionId;

            try
            {
                transactionId = _coordinator.GetCurrent(RequestorId);
            }
            catch (TransactionException)
            {
                return;
            }

            var validator = _validationProvider.GetVoucherValidator();

            if (_noteAcceptor?.WasStackingOnLastPowerUp ?? false)
            {
                Decline(transaction, transactionId, VoucherInExceptionCode.PowerFailure);
                validator?.CommitVoucher(transaction);
                return;
            }

            Logger.Info(
                $"Attempting to recover note in state {_noteAcceptor?.LastDocumentResult} from transaction: {transaction}");

            switch (_noteAcceptor?.LastDocumentResult)
            {
                case DocumentResult.Escrowed:
                    // The note should have been returned at startup, but it may be device dependent
                    _noteAcceptor.Return().ContinueWith(
                        async r =>
                        {
                            if (r.Result)
                            {
                                Logger.Info($"Returned note for transaction: {transaction}");

                                Decline(transaction, transactionId, VoucherInExceptionCode.None);
                            }
                            else
                            {
                                if (await _noteAcceptor.AcceptNote())
                                {
                                    Accept(transaction, transactionId);

                                    if (validator != null)
                                    {
                                        await validator.StackedVoucher(transaction);
                                }
                                }
                                else
                                {
                                    Decline(transaction, transactionId, VoucherInExceptionCode.NoteAcceptorFailure);
                                }
                            }

                            validator?.CommitVoucher(transaction);
                        });
                    break;
                case DocumentResult.Stacked:
                    if (_properties.GetValue(AccountingConstants.IgnoreVoucherStackedDuringReboot, false))
                    {
                        Decline(transaction, transactionId, VoucherInExceptionCode.None);
                    }
                    else
                    {
                        Accept(transaction, transactionId);

                        validator?.StackedVoucher(transaction).Wait();
                    }

                    validator?.CommitVoucher(transaction);
                    break;
                default:
                    Decline(transaction, transactionId);

                    validator?.CommitVoucher(transaction);
                    break;
            }
        }

        private static string GetCreditLimitExceededRejectionMessage(string rejectionResourceKey)
        {
            return string.Format(
                GetRejectionMessage(rejectionResourceKey),
                ServiceManager.GetInstance().GetService<IBank>().Limit.MillicentsToDollars());
        }

        private static string GetRejectionMessage(string rejectionResourceKey)
        {
            var baseMessage = Localizer.For(CultureFor.Player).GetString(ResourceKeys.VoucherRejected);
            var rejectionMessage = Localizer.For(CultureFor.Player).GetString(rejectionResourceKey);

            return $"{baseMessage} - {rejectionMessage}";
        }

        private async Task Handle(VoucherEscrowedEvent evt, CancellationToken token)
        {
            //TODO: Handle this better
            if (_properties.GetValue(ApplicationConstants.NoteAcceptorDiagnosticsKey, false))
            {
                Logger.Debug("Note acceptor diagnostics enabled: ignoring VoucherEscrowedEvent");
                return;
            }

            if (!_properties.GetValue(PropertyKey.VoucherIn, false))
            {
                Logger.Debug("Voucher in is disabled");
                await Reject(VoucherInExceptionCode.TicketingDisabled);
                return;
            }

            if (evt.Barcode.Length != BarcodeLength)
            {
                Logger.Debug($"Barcode does not meet minimum length - {evt}");
                await Reject(VoucherInExceptionCode.InvalidTicket);
                return;
            }

            var validator = _validationProvider.GetVoucherValidator();
            if (validator == null || !validator.CanValidateVouchersIn)
            {
                Logger.Info($"No validator or validation is currently not allowed - {evt}");
                await Reject(VoucherInExceptionCode.Other);
                return;
            }

            var transaction = new VoucherInTransaction(evt.NoteAcceptorId, DateTime.UtcNow, evt.Barcode)
            {
                State = VoucherState.Pending
            };

            Guid transactionId;

            using (var scope = _storage.ScopedTransaction())
            {
                transactionId = _coordinator.RequestTransaction(
                    RequestorId,
                    RequestTimeoutLength,
                    TransactionType.Write);
                if (transactionId == Guid.Empty)
                {
                    Logger.Info($"Failed to acquire a transaction - {evt}");
                    await Reject(transaction, transactionId);
                    return;
                }

                // To prevent the log from filling up with rejected vouchers if the previous transaction was rejected and has been committed
                //  the transaction will be overwritten with a new transaction Id
                var previous = _transactions.RecallTransactions<VoucherInTransaction>()
                    .OrderByDescending(t => t.TransactionId).FirstOrDefault();
                if (previous != null && previous.State == VoucherState.Rejected && previous.CommitAcknowledged)
                {
                    // The log sequence from the previous transaction must be used
                    transaction.LogSequence = previous.LogSequence;
                    transaction.VoucherSequence = previous.VoucherSequence;

                    _transactions.OverwriteTransaction(previous.TransactionId, transaction);
                }
                else
                {
                    // Unique log sequence number assigned by the EGM; a series that strictly increases by 1 (one) starting at 1 (one)
                    transaction.LogSequence = _idProvider.GetNextLogSequence<VoucherBaseTransaction>();

                    // At this point we're committed and must be sent to the host
                    _transactions.AddTransaction(transaction);
                }

                scope.Complete();
            }

            _bus.Publish(new VoucherRedemptionRequestedEvent(transaction));

            var voucherAmount = await validator.RedeemVoucher(transaction);

            _transactions.UpdateTransaction(transaction);

            _bus.Publish(new VoucherAuthorizedEvent(transaction));

            if (transaction.Exception == (int)VoucherInExceptionCode.None)
            {
                transaction.Exception = (int)ValidateAmount(transaction.Amount);
            }

            if (transaction.Exception != (int)VoucherInExceptionCode.None)
            {
                await Reject(transaction, transactionId);
            }
            else if (_noteAcceptor != null && await _noteAcceptor.AcceptTicket() && !TimedOutByHost)
            {
                Accept(transaction, transactionId, voucherAmount);

                await validator.StackedVoucher(transaction);
            }
            else
            {
                Decline(transaction, transactionId, VoucherInExceptionCode.NoteAcceptorFailure);
            }

            _properties.SetProperty(AccountingConstants.IsVoucherRedemptionTimedOut, false);

            validator.CommitVoucher(transaction);
        }

        private void Handle(VoucherReturnedEvent evt)
        {
            var transaction = _transactions.RecallTransactions<VoucherInTransaction>()
                .LastOrDefault(t => t.State == VoucherState.Pending);
            if (transaction != null)
            {
                using (var scope = _storage.ScopedTransaction())
                {
                    transaction.Amount = 0;
                    transaction.State = VoucherState.Rejected;
                    transaction.Exception = (int)VoucherInExceptionCode.NoteAcceptorFailure;

                    _transactions.UpdateTransaction(transaction);

                    var transactionId = _coordinator.GetCurrent(RequestorId);

                    _coordinator.ReleaseTransaction(transactionId);

                    scope.Complete();
                }

                DisplayMessage(VoucherInExceptionCode.NoteAcceptorFailure);
            }
            else
            {
                _coordinator.AbandonTransactions(RequestorId);
            }
        }

        private void Handle(DocumentRejectedEvent evt)
        {
            _coordinator.AbandonTransactions(RequestorId);
        }

        private void Handle(ServiceAddedEvent evt)
        {
            if (evt.ServiceType == typeof(INoteAcceptor))
            {
                _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            }
        }

        private async Task Reject(VoucherInExceptionCode exceptionCode)
        {
            await Reject(null, Guid.Empty, exceptionCode);
        }

        private async Task Reject(
            VoucherInTransaction transaction,
            Guid transactionId,
            VoucherInExceptionCode exceptionCode = VoucherInExceptionCode.Other)
        {
            if (_noteAcceptor == null || !await _noteAcceptor.Return())
            {
                Logger.Error("Failed to return document");
            }

            using (var scope = _storage.ScopedTransaction())
            {
                if (transaction != null)
                {
                    transaction.Amount = 0;
                    transaction.State = VoucherState.Rejected;
                    exceptionCode = (VoucherInExceptionCode)transaction.Exception;

                    if (transaction.TransactionId == 0)
                    {
                        _transactions.AddTransaction(transaction);
                    }
                    else
                    {
                        _transactions.UpdateTransaction(transaction);
                    }

                    _coordinator.ReleaseTransaction(transactionId);
                }

                _meters.GetMeter(AccountingMeters.VouchersRejectedCount).Increment(1);
                scope.Complete();
            }

            DisplayMessage(exceptionCode);

            _bus.Publish(new VoucherRejectedEvent(transaction));
        }

        private void Accept(VoucherInTransaction transaction, Guid transactionId, VoucherAmount amount = null)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                using (_lockManager.AcquireExclusiveLock(GetMetersToUpdate()))
                {
                    if (amount == null)
                    {
                        _bank.Deposit(transaction.TypeOfAccount, transaction.Amount, transactionId);
                    }
                    else
                    {
                        amount.Deposit(_bank, transactionId);
                    }

                    UpdateMeters(transaction, amount);
                }

                UpdateLaundryLimit(transaction);

                transaction.State = VoucherState.Redeemed;
                transaction.Exception = (int)VoucherInExceptionCode.None;

                _transactions.UpdateTransaction(transaction);

                _coordinator.ReleaseTransaction(transactionId);

                scope.Complete();
            }

            DisplayMessage((VoucherInExceptionCode)transaction.Exception, transaction.Amount);

            _bus.Publish(new VoucherRedeemedEvent(transaction));

            IEnumerable<IMeter> GetMetersToUpdate()
            {
                return new List<IMeter>()
                {
                    _meters.GetMeter(AccountingMeters.VoucherInNonCashableAmount),
                    _meters.GetMeter(AccountingMeters.VoucherInNonCashableCount),
                    _meters.GetMeter(AccountingMeters.VoucherInCashableAmount),
                    _meters.GetMeter(AccountingMeters.VoucherInCashableCount),
                    _meters.GetMeter(AccountingMeters.VoucherInCashablePromoAmount),
                    _meters.GetMeter(AccountingMeters.VoucherInCashablePromoCount),
                    _meters.GetMeter(AccountingMeters.DocumentsAcceptedCount)
                };
            }
        }

        private void Decline(
            VoucherInTransaction transaction,
            Guid transactionId,
            VoucherInExceptionCode exceptionCode = VoucherInExceptionCode.Other)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                transaction.Amount = 0;
                transaction.State = VoucherState.Rejected;
                transaction.Exception = (int)exceptionCode;

                _transactions.UpdateTransaction(transaction);

                _coordinator.ReleaseTransaction(transactionId);

                _coordinator.AbandonTransactions(RequestorId);

                _meters.GetMeter(AccountingMeters.VouchersRejectedCount).Increment(1);

                scope.Complete();
            }

            DisplayMessage((VoucherInExceptionCode)transaction.Exception);

            _bus.Publish(new VoucherRejectedEvent(transaction));
        }

        private VoucherInExceptionCode ValidateAmount(long amount)
        {
            if (amount == 0)
            {
                Logger.Info("Amount is zero, returning document");

                return VoucherInExceptionCode.ZeroAmount;
            }

            if (!CheckMaxCreditMeter(amount))
            {
                Logger.Info("Amount exceeds credit limit, returning document");

                return VoucherInExceptionCode.CreditLimitExceeded;
            }

            // Don't use the current voucher in the calculation
            // This allows a voucher > limit to be accepted
            if (!CheckLaundry(0))
            {
                Logger.Info("Amount exceeds laundry limit, returning document");

                return VoucherInExceptionCode.LaundryLimitExceeded;
            }

            if (!CheckVoucherInLimit(amount))
            {
                Logger.Info("Amount exceeds voucher limit, returning document");

                return VoucherInExceptionCode.VoucherInLimitExceeded;
            }

            return VoucherInExceptionCode.None;
        }

        private void DisplayMessage(VoucherInExceptionCode exceptionCode, long acceptedAmount = 0)
        {
            if (exceptionCode == VoucherInExceptionCode.None)
            {
                _messageDisplay.DisplayMessage(
                    new DisplayableMessage(
                        () => Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.VoucherAccepted) +
                              " " + acceptedAmount.MillicentsToDollars().FormattedCurrencyString(),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(VoucherRedeemedEvent)));
            }
            else if (_exceptionDisplayMessageMap.ContainsKey(exceptionCode))
            {
                _messageDisplay.DisplayMessage(_exceptionDisplayMessageMap[exceptionCode]);
            }
        }

        private bool CheckVoucherInLimit(long amount)
        {
            var voucherInLimit = _properties.GetValue(
                AccountingConstants.VoucherInLimit,
                AccountingConstants.DefaultVoucherInLimit);
            return voucherInLimit >= amount;
        }

        private void UpdateMeters(VoucherBaseTransaction transaction, VoucherAmount voucherAmount)
        {
            if (voucherAmount != null && transaction.TypeOfAccount.Equals(AccountType.Cashable) &&
                voucherAmount.PromoAmount > 0)
            {
                _meters.GetMeter(AccountingMeters.VoucherInNonCashableAmount).Increment(voucherAmount.PromoAmount);
                _meters.GetMeter(AccountingMeters.VoucherInNonCashableCount).Increment(1);

                if (voucherAmount.CashAmount > 0)
                {
                    _meters.GetMeter(AccountingMeters.VoucherInCashableAmount).Increment(voucherAmount.CashAmount);
                }
            }
            else
            {
                switch (transaction.TypeOfAccount)
                {
                    case AccountType.Cashable:
                        _meters.GetMeter(AccountingMeters.VoucherInCashableAmount).Increment(transaction.Amount);
                        _meters.GetMeter(AccountingMeters.VoucherInCashableCount).Increment(1);
                        break;
                    case AccountType.Promo:
                        _meters.GetMeter(AccountingMeters.VoucherInCashablePromoAmount).Increment(transaction.Amount);
                        _meters.GetMeter(AccountingMeters.VoucherInCashablePromoCount).Increment(1);
                        break;
                    case AccountType.NonCash:
                        _meters.GetMeter(AccountingMeters.VoucherInNonCashableAmount).Increment(transaction.Amount);
                        _meters.GetMeter(AccountingMeters.VoucherInNonCashableCount).Increment(1);
                        break;
                }
            }

            _meters.GetMeter(AccountingMeters.DocumentsAcceptedCount).Increment(1);
        }
    }
}