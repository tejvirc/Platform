namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Kernel.Contracts.MessageDisplay;
    using Kernel.MessageDisplay;
    using Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using log4net;

    public class CurrencyInProvider : TransferInProviderBase, IService, IDisposable
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private const string RecoverDataKey = @"RecoverData";
        private const int RequestTimeoutLength = 1000; // It's in milliseconds

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Guid RequestorId = new Guid("{8C475AA8-687F-47CE-9056-39157FA9D6A8}");

        private readonly IPersistentStorageAccessor _accessor;
        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly ITransactionCoordinator _coordinator;
        private readonly IMeterManager _meters;
        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storage;
        private readonly IMessageDisplay _messageDisplay;
        private readonly ITransactionHistory _transactions;
        private readonly IIdProvider _idProvider;
        private readonly IValidationProvider _validationProvider;
        private readonly string _defaultCurrencyId;
        private readonly double _multiplier;

        private INoteAcceptor _noteAcceptor;
        private bool _disposed;

        private readonly Dictionary<CurrencyInExceptionCode, IDisplayableMessage> _exceptionDisplayMessageMap =
            new Dictionary<CurrencyInExceptionCode, IDisplayableMessage>
            {
                {
                    CurrencyInExceptionCode.CreditLimitExceeded, new DisplayableMessage(
                        () => GetCreditLimitExceededRejectionMessage(ResourceKeys.CreditLimitExceeded),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent),
                        new Guid("{D8857630-CA71-47BB-82CC-EDDDCFC0490B}"))
                },
                {
                    CurrencyInExceptionCode.CreditInLimitExceeded, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.CreditInLimitExceeded),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent),
                        new Guid("{D166BEBF-28CB-45D3-8B0B-5DA685DCBC22}"))
                },
                {
                    CurrencyInExceptionCode.LaundryLimitExceeded, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.SessionLimitReached),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent),
                        new Guid("{7DF0A475-A0A3-40B5-974D-CE11E5CAF08F}"))
                },
                {
                    CurrencyInExceptionCode.InvalidBill, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.InvalidBill),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent),
                        new Guid("{D13ABE3E-F009-4A43-A53B-F25690831F2D}"))
                },
                {
                    CurrencyInExceptionCode.PowerFailure, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.PowerFailure),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent),
                        new Guid("{455929AD-B4B6-47AA-A616-CFFC81A8E293}"))
                },
                {
                    CurrencyInExceptionCode.NoteAcceptorFailure, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.NoteAcceptorFailure),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent),
                        new Guid("{03C4FAE0-1E72-4A35-93CE-C6B20E084FB3}"))
                },
                {
                    CurrencyInExceptionCode.Other, new DisplayableMessage(
                        () => GetRejectionMessage(ResourceKeys.NoteAcceptorFailure),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent),
                        new Guid("{08CC0588-AF86-47FF-A1EB-BFF91179BED9}"))
                },
                {
                    CurrencyInExceptionCode.UnknownDocument, new DisplayableMessage(
                        () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.DocumentRejected),
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(DocumentRejectedEvent),
                        new Guid("{56227F90-8E0F-4F1F-9334-3372E7213540}"))
                }
            };

        public CurrencyInProvider()
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
                ServiceManager.GetInstance().GetService<IValidationProvider>())
        {
        }

        public CurrencyInProvider(
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
            IValidationProvider validationProvider)
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

            var blockName = GetType().ToString();
            _accessor = _storage.GetAccessor(Level, blockName);

            _multiplier = _properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);
            _defaultCurrencyId = _properties.GetValue(ApplicationConstants.CurrencyId, string.Empty);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(CurrencyInProvider).Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(CurrencyInProvider) };

        public void Initialize()
        {
            _bus.Subscribe<ServiceAddedEvent>(this, Handle);
            _bus.Subscribe<CurrencyEscrowedEvent>(this, Handle);
            _bus.Subscribe<DocumentRejectedEvent>(this, Handle);
            _bus.Subscribe<CurrencyReturnedEvent>(this, Handle);
            _bus.Subscribe<MissedStartupEvent>(this, Handle);
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
            var evt = GetRecoveryData();
            if (evt == null)
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
                WriteRecoveryData(null);
                return;
            }

            var transaction = CreateBillTransaction(evt);

            var lastSavedTransaction = _transactions.RecallTransactions<BillTransaction>()
                .OrderByDescending(t => t.TransactionId).FirstOrDefault();

            if (lastSavedTransaction != null && lastSavedTransaction.State != CurrencyState.Accepted)
            {
                transaction.LogSequence = lastSavedTransaction.LogSequence;
                transaction.TransactionId = lastSavedTransaction.TransactionId;
            }
            else if (transaction != null)
            {
                transaction.LogSequence = _idProvider.GetNextLogSequence<BillTransaction>();
            }
            else
            {
                Rollback(transactionId);
                return;
            }

            if (_noteAcceptor?.WasStackingOnLastPowerUp ?? false)
            {
                transaction.State = CurrencyState.Rejected;
                transaction.Exception = (int)CurrencyInExceptionCode.PowerFailure;
                Decline(transactionId, transaction);

                return;
            }

            Logger.Info(
                $"Attempting to recover note in state {_noteAcceptor?.LastDocumentResult} from event: {evt} , for transaction ID: {transaction.TransactionId}");

            var currencyValidator = _validationProvider.GetCurrencyValidator();

            switch (_noteAcceptor?.LastDocumentResult)
            {
                case DocumentResult.Escrowed:
                    // The note should have been returned at startup, but it may be device dependent
                    _noteAcceptor.Return().ContinueWith(
                        async r =>
                        {
                            if (r.Result || !await _noteAcceptor.AcceptNote())
                            {
                                Logger.Info($"Returned note for event: {evt}");
                                transaction.State = CurrencyState.Returned;
                                transaction.Exception = (int)CurrencyInExceptionCode.NoteAcceptorFailure;
                                Decline(transactionId, transaction);
                            }
                            else
                            {
                                await Commit(evt, transaction, transactionId, currencyValidator);
                            }
                        });
                    break;

                case DocumentResult.Stacked:
                    transaction.State = CurrencyState.Accepted;
                    Commit(evt, transaction, transactionId, currencyValidator).Wait();
                    break;
                case DocumentResult.Rejected:
                    transaction.State = CurrencyState.Rejected;
                    transaction.Exception = (int)CurrencyInExceptionCode.NoteAcceptorFailure;
                    Decline(transactionId, transaction);
                    break;

                default:
                    transaction.State = CurrencyState.Returned;
                    transaction.Exception = (int)CurrencyInExceptionCode.NoteAcceptorFailure;
                    Decline(transactionId, transaction);
                    break;
            }
        }

        private static string GetCreditLimitExceededRejectionMessage(string rejectionResourceKey)
        {
            // For CreditLimit exceeded message we need Limit in some markets
            return string.Format(
                GetRejectionMessage(rejectionResourceKey),
                ServiceManager.GetInstance().GetService<IBank>().Limit.MillicentsToDollars());
        }

        private static string GetRejectionMessage(string rejectionResourceKey)
        {
            var baseMessage = Localizer.For(CultureFor.Player).GetString(ResourceKeys.BillRejected);
            var rejectionMessage = Localizer.For(CultureFor.Player).GetString(rejectionResourceKey);

            return $"{baseMessage} - {rejectionMessage}";
        }

        private static byte[] ToByteArray(CurrencyEscrowedEvent evt)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                formatter.Serialize(stream, evt);

                return stream.ToArray();
            }
        }

        private async Task Handle(CurrencyEscrowedEvent evt, CancellationToken token)
        {
            //TODO: Handle this better
            if (_properties.GetValue(ApplicationConstants.NoteAcceptorDiagnosticsKey, false))
            {
                Logger.Debug("Note acceptor diagnostics enabled: ignoring CurrencyEscrowedEvent");
                return;
            }

            var transaction = CreateBillTransaction(evt);

            Guid transactionId;
            var previous = _transactions.RecallTransactions<BillTransaction>()
                .OrderByDescending(t => t.TransactionId).FirstOrDefault();
            using (var scope = _storage.ScopedTransaction())
            {
                transactionId = _coordinator.RequestTransaction(
                    RequestorId,
                    RequestTimeoutLength,
                    TransactionType.Write);

                if (transactionId == Guid.Empty)
                {
                    Logger.Info("Failed to acquire a transaction, returning document");

                    if (!await _noteAcceptor.Return())
                    {
                        Logger.Error("Failed to return document");
                    }

                    return;
                }

                WriteRecoveryData(evt);
                if (previous != null && (previous.State == CurrencyState.Rejected || previous.State == CurrencyState.Returned))
                {
                    transaction.LogSequence = previous.LogSequence;
                    _transactions.OverwriteTransaction(previous.TransactionId, transaction);
                }
                else
                {
                    // Unique log sequence number assigned by the EGM; a series that strictly increases by 1 (one) starting at 1 (one).
                    transaction.LogSequence = _idProvider.GetNextLogSequence<BillTransaction>();
                    _transactions.AddTransaction(transaction);
                }

                scope.Complete();
            }

            var amount = (long)(evt.Note.Value * _multiplier);

            var currencyValidator = _validationProvider.GetCurrencyValidator();
            if (currencyValidator != null)
            {
                transaction.Exception = (int)await currencyValidator.ValidateNote(evt.Note.Value);
            }

            if (transaction.Exception == (int)CurrencyInExceptionCode.None)
            {
                transaction.Exception = (int)ValidateAmount(evt.Note.Value, amount);
            }

            if (transaction.Exception != (int)CurrencyInExceptionCode.None)
            {
                transaction.State = CurrencyState.Rejected;

                if (!await _noteAcceptor.Return())
                {
                    transaction.Exception = (int)CurrencyInExceptionCode.NoteAcceptorFailure;

                    Logger.Error("Failed to return document");
                }

                _meters.GetMeter(AccountingMeters.BillsRejectedCount).Increment(1);

                Decline(transactionId, transaction);
            }
            else
            {
                transaction.State = CurrencyState.Accepting;

                _bus.Publish(new CurrencyInStartedEvent(evt.Note));

                if (await _noteAcceptor.AcceptNote())
                {
                    transaction.State = CurrencyState.Accepted;
                    transaction.Exception = (int)CurrencyInExceptionCode.None;

                    if (!await Commit(evt, transaction, transactionId, currencyValidator))
                    {
                            return;
                    }

                    Logger.Info($"Accepted note: {evt}");

                    DisplayMessage((CurrencyInExceptionCode)transaction.Exception, amount);
                }
                else
                {
                    transaction.State = CurrencyState.Returned;
                    transaction.Exception = (int)CurrencyInExceptionCode.NoteAcceptorFailure;

                    Decline(transactionId, transaction);

                    Logger.Error($"Failed to accepted note: {evt}");
                }
            }
        }

        private async Task<bool> Commit(
            CurrencyEscrowedEvent evt,
            BillTransaction transaction,
            Guid transactionId,
            ICurrencyValidator currencyValidator)
        {
            if (currencyValidator != null)
            {
                if (!await currencyValidator.StackedNote(evt.Note.Value))
                {
                    transaction.Exception = (int)CurrencyInExceptionCode.Other;

                    Decline(transactionId, transaction);

                    Logger.Error($"Failed to credit note: {evt}");

                    return false;
                }
            }

            Commit(transactionId, transaction, evt.Note);
            return true;
        }

        private void Handle(DocumentRejectedEvent evt)
        {
            if (_properties.GetValue(ApplicationConstants.NoteAcceptorDiagnosticsKey, false))
            {
                Logger.Debug("Note acceptor diagnostics enabled: ignoring DocumentRejectedEvent");
                return;
            }

            _meters.GetMeter(AccountingMeters.DocumentsRejectedCount).Increment(1);

            _bus.Publish(new CurrencyInCompletedEvent(0L));

            DisplayMessage(CurrencyInExceptionCode.UnknownDocument);
        }

        private void Handle(CurrencyReturnedEvent evt)
        {
            var transaction = _transactions.RecallTransactions<BillTransaction>()
                .LastOrDefault(t => t.State == CurrencyState.Pending);
            if (transaction != null)
            {
                var transactionId = _coordinator.GetCurrent(RequestorId);

                transaction.State = CurrencyState.Returned;
                transaction.Exception = (int)CurrencyInExceptionCode.NoteAcceptorFailure;

                Decline(transactionId, transaction);
            }
            else
            {
                Logger.Info("Failed to locate a pending CurrencyInTransaction");
            }
        }

        private void Handle(MissedStartupEvent missedEvent)
        {
            if (missedEvent.MissedEvent is DocumentRejectedEvent evt)
            {
                Handle(evt);
            }
        }

        private void Handle(ServiceAddedEvent evt)
        {
            if (evt.ServiceType == typeof(INoteAcceptor))
            {
                _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            }
        }

        private CurrencyInExceptionCode ValidateAmount(int denom, long amount)
        {
            if (!_noteAcceptor.DenomIsValid(denom))
            {
                Logger.Info("Denom value is not valid, returning document");

                return CurrencyInExceptionCode.InvalidBill;
            }

            if (!CheckMaxCreditMeter(amount))
            {
                Logger.Info("Note amount would exceed credit limit, returning document");

                return CurrencyInExceptionCode.CreditLimitExceeded;
            }

            if (!CheckMaxCreditsIn(amount))
            {
                Logger.Info("Note amount would exceed max credits in, returning document");

                return CurrencyInExceptionCode.CreditInLimitExceeded;
            }

            if (!CheckLaundry(amount))
            {
                Logger.Info("Note amount would exceed laundry limit, returning document");

                return CurrencyInExceptionCode.LaundryLimitExceeded;
            }

            return CurrencyInExceptionCode.None;
        }

        private void DisplayMessage(CurrencyInExceptionCode exceptionCode, long acceptedAmount = 0)
        {
            if (exceptionCode == CurrencyInExceptionCode.None)
            {
                _messageDisplay.DisplayMessage(
                    new DisplayableMessage(
                        ResourceKeys.BillAccepted,
                        CultureProviderType.Player,
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent))
                    {
                        Params = new object[] { acceptedAmount.MillicentsToDollars().FormattedCurrencyString() }
                    });
            }
            else if (_exceptionDisplayMessageMap.ContainsKey(exceptionCode))
            {
                _messageDisplay.DisplayMessage(_exceptionDisplayMessageMap[exceptionCode]);
            }
        }

        private bool CheckMaxCreditsIn(long amount)
        {
            if (ServiceManager.GetInstance().TryGetService<ICurrencyInValidator>()?.IsValid(amount) ?? true)
            {
                return true;
            }

            Logger.Warn(
                $"Amount of {amount} exceeds Max Credits In of {_properties.GetValue(PropertyKey.MaxCreditsIn, AccountingConstants.DefaultMaxTenderInLimit)}");

            return false;
        }

        private BillTransaction CreateBillTransaction(CurrencyEscrowedEvent evt)
        {
            return CreateBillTransaction(
                evt.NoteAcceptorId,
                evt.Note.Value,
                evt.Note.CurrencyCode,
                CurrencyInExceptionCode.None);
        }

        private BillTransaction CreateBillTransaction(
            int deviceId,
            long denomination,
            ISOCurrencyCode currencyCode,
            CurrencyInExceptionCode exceptionCode)
        {
            var currencyId = Enum.GetName(typeof(ISOCurrencyCode), currencyCode);
            if (string.IsNullOrEmpty(currencyId))
            {
                currencyId = _defaultCurrencyId;
            }

            var amount = (long)(denomination * _multiplier);

            return new BillTransaction(
                currencyId.ToCharArray(0, 3),
                deviceId,
                DateTime.UtcNow,
                amount,
                CurrencyState.Pending,
                (int)exceptionCode) { Denomination = denomination };
        }

        private void Commit(Guid transactionId, BillTransaction transaction, INote note)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                transaction.Accepted = DateTime.UtcNow;

                _transactions.UpdateTransaction(transaction);

                _bank.Deposit(transaction.TypeOfAccount, transaction.Amount, transactionId);

                _meters.GetMeter($"BillCount{transaction.Denomination}s").Increment(1);

                _meters.GetMeter(AccountingMeters.DocumentsAcceptedCount).Increment(1);

                UpdateLaundryLimit(transaction);

                WriteRecoveryData(null);

                _coordinator.ReleaseTransaction(transactionId);

                scope.Complete();
            }

            _bus.Publish(new CurrencyInCompletedEvent(transaction.Amount, note, transaction));
        }

        private void Decline(Guid transactionId, BillTransaction transaction)
        {
            if (transactionId != Guid.Empty)
            {
                using (var scope = _storage.ScopedTransaction())
                {
                    _transactions.UpdateTransaction(transaction);

                    WriteRecoveryData(null);

                    _coordinator.ReleaseTransaction(transactionId);

                    scope.Complete();
                }
            }

            DisplayMessage((CurrencyInExceptionCode)transaction.Exception);
            _bus.Publish(new CurrencyInCompletedEvent(0L));
            Logger.Info($"Declined BillTransaction: {transactionId}");
        }

        private void Rollback(Guid transactionId)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                _coordinator.AbandonTransactions(RequestorId);

                WriteRecoveryData(null);

                scope.Complete();
            }

            Logger.Info($"Rolled back transaction {transactionId}");
        }

        private void WriteRecoveryData(CurrencyEscrowedEvent evt)
        {
            using (var transaction = _accessor.StartTransaction())
            {
                transaction[RecoverDataKey] = evt != null ? ToByteArray(evt) : null;
                transaction.Commit();
            }
        }

        private CurrencyEscrowedEvent GetRecoveryData()
        {
            var data = (byte[])_accessor[RecoverDataKey];
            if (data?.Length > 2)
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();

                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;

                    if (formatter.Deserialize(stream) is CurrencyEscrowedEvent evt)
                    {
                        return evt;
                    }
                }
            }

            return null;
        }
    }
}