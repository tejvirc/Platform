namespace Vgt.Client12.Testing.Tools
{
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Kernel.Contracts.Events;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using DisabledEvent = Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor.DisabledEvent;
    using EnabledEvent = Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor.EnabledEvent;

    /// <summary>
    ///     Listens for currency-escrowed events and, upon receiving one,
    ///     attempts to start a new transaction in which it will work with
    ///     the bank and note acceptor to stack the document and deposit
    ///     the money.
    /// </summary>
    public class DebugCurrencyHandler : BaseRunnable
    {
        private const string DenominationMeterNamePrefix = "BillCount";
        private const string DenominationMeterNamePostfix = "s";
        private const int TimeoutInMillisecondsForATransactionRequest = 1000;
        private const int ShowModeHandpayLockupDuration = 5000;

        private const PersistenceLevel Level = PersistenceLevel.Transient;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid RequestorId = new Guid("{10795E25-1B46-45d3-94EC-993D47B14694}");
        private readonly long _showModeMaxBankReset = 1000M.DollarsToMillicents();

        private Timer _showModeLockupTimer;

        private CheckCreditsStrategy _checkCreditsIn;

        private IMessageDisplay _messageDisplay;

        private bool _noteAcceptorEnabled;

        private bool _showModeActive;

        // Default _restrictDebugCreditsIn is off (false) to allow debug credits in to be inserted over the configured max credits in limit.
        // Use F5 key to toggle on/off.  When on, debug credits in will be restricted to the configured max credits in limit the same as when 
        // real currency with the note acceptor.
        private bool _restrictDebugCreditsIn;

        private Transaction _transaction;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            _checkCreditsIn = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None);

            // Get or create block of NVRam
            var storageService = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            var blockName = GetType().ToString();
            if (storageService.BlockExists(blockName))
            {
                Retrieve();
            }
            else
            {
                storageService.CreateBlock(Level, blockName, 1);
            }

            _messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();

            Log.Debug("Initialized");
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            Log.Debug("Run started");

            SubscribeToEvents();
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            Log.Debug("Stopping...");

            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);

            Log.Debug("Stopped");
        }

        private static void VerifyBalance(long expectedBalance, AccountType creditType)
        {
            var bank = ServiceManager.GetInstance().GetService<IBank>();

            var currentBalance = bank.QueryBalance(creditType);
            if (currentBalance != expectedBalance)
            {
                var builder = new StringBuilder();
                builder.AppendFormat(
                    "Unexpected bank balance [{0}] after debug currency transaction.  Expected {1}.",
                    currentBalance,
                    expectedBalance);
                Log.Error(builder.ToString());
                throw new DebugCurrencyException(builder.ToString());
            }
        }

        private static BillTransaction RecordBillTransaction(long amount, CurrencyInExceptionCode exception = CurrencyInExceptionCode.Virtual)
        {
            var serviceManager = ServiceManager.GetInstance();

            // Build the meter name, which is "prefix + denom + postfix".
            // The denom should be in bill units (eg. dollars), whereas the 
            // amount' is in currency units (eg. cents or millicents).
            var propertiesManager = serviceManager.GetService<IPropertiesManager>();
            var denominationToCurrencyMultiplier =
                (double)propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, null);
            var denomination = (long)(amount / denominationToCurrencyMultiplier);
            var meterName = DenominationMeterNamePrefix + denomination + DenominationMeterNamePostfix;
            // Increment the occurrence meter by 1
            var meterManager = serviceManager.GetService<IMeterManager>();
            var meter = meterManager.GetMeter(meterName);
            meter.Increment(1);

            // Get the transaction and log sequence Ids
            var history = serviceManager.GetService<ITransactionHistory>();

            var billTransaction = CreateBillTransaction(amount, denomination, exception);

            // Save the transaction in history.
            // *NOTE* Need to do this last so that recovery logic will record the transaction again if we reboot before the transaction is saved.
            history.AddTransaction(billTransaction);
            return billTransaction;
        }

        private static VoucherInTransaction RecordVoucherTransaction(long amount, AccountType accountType)
        {
            var serviceManager = ServiceManager.GetInstance();

            // Build the meter name, which is "prefix + denom + postfix".
            // The denom should be in bill units (eg. dollars), whereas the 
            // amount' is in currency units (eg. cents or millicents).
            var propertiesManager = serviceManager.GetService<IPropertiesManager>();
            var denominationToCurrencyMultiplier =
                (double)propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, null);
            var denomination = (long)(amount / denominationToCurrencyMultiplier);
            var meterName = DenominationMeterNamePrefix + denomination + DenominationMeterNamePostfix;
            // Increment the occurrence meter by 1
            var meterManager = serviceManager.GetService<IMeterManager>();
            var meter = meterManager.GetMeter(meterName);
            meter.Increment(1);

            // Get the transaction and log sequence Ids
            var history = serviceManager.GetService<ITransactionHistory>();

            VoucherInTransaction transaction = CreateVoucherInTransaction(amount, accountType);

            // Save the transaction in history.
            // *NOTE* Need to do this last so that recovery logic will record the transaction again if we reboot before the transaction is saved.
            history.AddTransaction(transaction);
            return transaction;
        }

        private static BillTransaction CreateBillTransaction(long amount, long denomination, CurrencyInExceptionCode exception)
        {
            var currencyId = ServiceManager.GetInstance()
                .GetService<IPropertiesManager>()
                .GetValue(ApplicationConstants.CurrencyId, string.Empty).ToCharArray(0, 3);

            // Create a bill transaction record for history.
            // For now, we only have one note acceptor device, so set deviceId to 1.
            var billTransaction = new BillTransaction(
                currencyId,
                1,
                DateTime.UtcNow,
                exception == CurrencyInExceptionCode.Virtual ? amount : 0L)
            {
                Denomination = denomination,
                Accepted = DateTime.UtcNow,
                State = CurrencyState.Accepted,
                Exception = (int)exception
            };

            return billTransaction;
        }

        private static VoucherInTransaction CreateVoucherInTransaction(long amount, AccountType accountType)
        {
            // Create a bill transaction record for history.
            // For now, we only have one note acceptor device, so set deviceId to 1.
            var voucherInTransaction = new VoucherInTransaction(
                1,
                DateTime.UtcNow,
                "1234")
            {
                Amount = amount,
                TypeOfAccount = accountType
            };

            return voucherInTransaction;
        }

        private static bool IsNoteEnabled(int denomination)
        {
            // Is the note acceptor service available?
            var noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            if (noteAcceptor == null)
            {
                // No, return note enabled.
                return false;
            }

            return noteAcceptor.Denominations.Contains(denomination);
        }

        private void HandleEvent(DebugCoinEvent coinEvent)
        {
            if (UpdateBalance(coinEvent.Denomination, AccountType.Cashable))
            {
                UpdateMeters(AccountType.Cashable, coinEvent.Denomination);
                var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                eventBus.Publish(new CurrencyInCompletedEvent(coinEvent.Denomination));
            }
        }

        private void HandleEvent(DebugNoteEvent debugNoteEvent)
        {
            if (!IsNoteEnabled(debugNoteEvent.Denomination))
            {
                Log.Debug($"Denomination is not active - {debugNoteEvent.Denomination}");
                return;
            }

            ResetLaundryLimit();

            var currencyValidator = ServiceManager.GetInstance().TryGetService<ICurrencyValidator>();
            if (currencyValidator != null)
            {
                if (currencyValidator.ValidateNote(debugNoteEvent.Denomination).Result !=
                    CurrencyInExceptionCode.None)
                {
                    Log.Debug("Debug note rejected by host");
                    return;
                }
            }

            var eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>();
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var denominationToCurrencyMultiplier =
                (double)propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, null);

            var amount = (long)(debugNoteEvent.Denomination * denominationToCurrencyMultiplier);

            if (!CheckLaundry(amount))
            {
                RecordBillTransaction(amount, CurrencyInExceptionCode.LaundryLimitExceeded);
                eventBus?.Publish(new CurrencyInCompletedEvent(0L));

                _messageDisplay.DisplayMessage(new DisplayableMessage(
                        () => "Bill Rejected - Session Limit Reached",
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        typeof(CurrencyInCompletedEvent),
                        new Guid("{7DF0A475-A0A3-40B5-974D-CE11E5CAF08F}")));
                return;
            }

            if (currencyValidator != null)
            {
                if (!currencyValidator.StackedNote(debugNoteEvent.Denomination).Result)
                {
                    return;
                }
            }

            if (!UpdateBalance(
                amount,
                AccountType.Cashable))
            {
                return;
            }

            var transaction = RecordBillTransaction(amount);

            UpdateLaundryLimit(transaction);

            var note = new Note()
            {
                Value = debugNoteEvent.Denomination,
                ISOCurrencySymbol = propertiesManager.GetValue(ApplicationConstants.CurrencyId, string.Empty)
            };

            eventBus?.Publish(new CurrencyStackedEvent(note));
            eventBus?.Publish(new CurrencyInCompletedEvent(amount, note, transaction));
        }

        private void HandleEvent(DebugAnyCreditEvent debugAnyCredit)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            var denominationToCurrencyMultiplier =
                (double)propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, null);
            var amount = (long)(debugAnyCredit.Amount * denominationToCurrencyMultiplier);

            if (UpdateBalance(amount, debugAnyCredit.CreditType))
            {
                var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                UpdateMeters(debugAnyCredit.CreditType, amount);

                var transaction = RecordVoucherTransaction(amount, debugAnyCredit.CreditType);
                if (transaction != null)
                {
                    eventBus?.Publish(new VoucherRedeemedEvent(transaction));
                }
            }
        }

        private bool UpdateBalance(long amount, AccountType creditType)
        {
            Log.Debug("Attempting to start a new transaction...");

            var serviceMgr = ServiceManager.GetInstance();
            var coordinator = serviceMgr.GetService<ITransactionCoordinator>();

            Transaction transaction;
            transaction.Guid = coordinator.RequestTransaction(
                RequestorId,
                TimeoutInMillisecondsForATransactionRequest,
                TransactionType.Write);
            if (Guid.Empty == transaction.Guid)
            {
                Log.Warn("Transaction coordinator denied the transaction.");
                return false;
            }

            try
            {
                Log.Info("Starting a new transaction...");

                var bank = serviceMgr.GetService<IBank>();

                transaction.Amount = amount;

                if (bank.CheckDeposit(creditType, transaction.Amount, transaction.Guid))
                {
                    var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                    var checkCreditsIn = propertiesManager.GetValue(
                        AccountingConstants.CheckCreditsIn,
                        CheckCreditsStrategy.None);

                    if (_restrictDebugCreditsIn)
                    {
                        if (!(ServiceManager.GetInstance().TryGetService<ICurrencyInValidator>()
                                  ?.IsValid(transaction.Amount) ??
                              true) || !CheckMaxCreditMeter(transaction.Amount))
                        {
                            Log.Warn($"Amount {transaction.Amount} failed limit check, ending transaction...");
                            return false;
                        }
                    }
                    else
                    {
                        if (checkCreditsIn != CheckCreditsStrategy.None)
                        {
                            propertiesManager.SetProperty(AccountingConstants.CheckCreditsIn, CheckCreditsStrategy.None);
                        }
                    }

                    Log.Debug($"Bank authorized amount {transaction.Amount}, depositing...");
                    ServiceManager.GetInstance().GetService<IEventBus>()?.Publish(new DebugCurrencyAcceptedEvent());

                    var currentBalance = bank.QueryBalance(creditType);

                    transaction.NewAccountBalance = currentBalance + transaction.Amount;
                    _transaction = transaction;
                    Persist();
                    bank.Deposit(creditType, transaction.Amount, transaction.Guid);

                    if (!_restrictDebugCreditsIn && checkCreditsIn != CheckCreditsStrategy.None)
                    {
                        propertiesManager.SetProperty(AccountingConstants.CheckCreditsIn, _checkCreditsIn);

                        ServiceManager.GetInstance().TryGetService<ICurrencyInValidator>()?.IsValid();
                    }

                    VerifyBalance(_transaction.NewAccountBalance, creditType);

                    ResetState();

                    return true;
                }

                Log.Warn($"Bank denied deposit of amount {transaction.Amount}, ending transaction...");

                return false;
            }
            finally
            {
                coordinator.ReleaseTransaction(transaction.Guid);
            }
        }

        private void UpdateMeters(AccountType type, long amount)
        {
            var meters = ServiceManager.GetInstance().TryGetService<IMeterManager>();

            switch (type)
            {
                case AccountType.Cashable:
                    meters.GetMeter(AccountingMeters.VoucherInCashableAmount).Increment(amount);
                    meters.GetMeter(AccountingMeters.VoucherInCashableCount).Increment(1);
                    break;
                case AccountType.Promo:
                    meters.GetMeter(AccountingMeters.VoucherInCashablePromoAmount).Increment(amount);
                    meters.GetMeter(AccountingMeters.VoucherInCashablePromoCount).Increment(1);
                    break;
                case AccountType.NonCash:
                    meters.GetMeter(AccountingMeters.VoucherInNonCashableAmount).Increment(amount);
                    meters.GetMeter(AccountingMeters.VoucherInNonCashableCount).Increment(1);
                    break;
            }

            meters.GetMeter(AccountingMeters.DocumentsAcceptedCount).Increment(1);
        }

        private IPersistentStorageAccessor GetStorageBlock()
        {
            var storageService = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            return storageService.GetBlock(GetType().ToString());
        }

        private void SubscribeToEvents()
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            eventBus.Subscribe<DebugNoteEvent>(this, ReceiveEvent);
            eventBus.Subscribe<DebugCoinEvent>(this, ReceiveEvent);
            eventBus.Subscribe<EnabledEvent>(this, OnNoteAcceptorEnabled);
            eventBus.Subscribe<DisabledEvent>(this, OnNoteAcceptorDisabled);
            eventBus.Subscribe<DebugAnyCreditEvent>(this, ReceiveEvent);
            eventBus.Subscribe<InitializationCompletedEvent>(this, InitializationCompletedEventHandler);

            _showModeActive = (bool)ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetProperty(ApplicationConstants.ShowMode, false);
            if (_showModeActive)
            {
                eventBus.Subscribe<GameIdleEvent>(this, _ => UpdateShowModeAccountBalance());
                eventBus.Subscribe<HandpayKeyOffPendingEvent>(this, ShowModeHandleEvent);
                eventBus.Subscribe<OperatorMenuExitedEvent>(this, _ => UpdateShowModeAccountBalance());
                eventBus.Subscribe<TransferOutCompletedEvent>(this, _ => UpdateShowModeAccountBalance());
            }
        }

        private void InitializationCompletedEventHandler(InitializationCompletedEvent obj)
        {
            Recover();

            var showMode = (bool)ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetProperty(ApplicationConstants.ShowMode, false);
            if (showMode)
            {
                Log.Info("Initialization completed, updating show mode account balance.");
                UpdateShowModeAccountBalance();
            }
        }

        private void OnNoteAcceptorEnabled(IEvent platformEvent)
        {
            _noteAcceptorEnabled = true;
        }

        private void OnNoteAcceptorDisabled(IEvent platformEvent)
        {
            _noteAcceptorEnabled = false;
        }

        private void Recover()
        {
            var coordinator =
                ServiceManager.GetInstance().GetService<ITransactionCoordinator>();

            // If we have valid transaction data, finish the transaction, else make sure we don't have any open transaction(s).
            if (Guid.Empty != _transaction.Guid && _transaction.Amount > 0 &&
                _transaction.NewAccountBalance >= _transaction.Amount)
            {
                Log.Debug("Recovering unfinished transaction");

                FinishRecoveredDeposit();
                coordinator.ReleaseTransaction(_transaction.Guid);
                ResetState();
            }
            else
            {
                Log.Debug("Recovering an idle state");

                ResetState();
                coordinator.AbandonTransactions(RequestorId);
            }
        }

        private void FinishRecoveredDeposit()
        {
            var serviceMgr = ServiceManager.GetInstance();

            Log.Debug("Recovered transaction");

            var bank = serviceMgr.GetService<IBank>();
            var originalBalance = _transaction.NewAccountBalance - _transaction.Amount;
            var currentBalance = bank.QueryBalance(AccountType.Cashable);
            if (currentBalance == originalBalance)
            {
                Log.Debug("Recovered transaction - making bank deposit");
                bank.Deposit(AccountType.Cashable, _transaction.Amount, _transaction.Guid);
                RecordBillTransaction(_transaction.Amount);
            }

            VerifyBalance(_transaction.NewAccountBalance, AccountType.Cashable);
        }

        private void ResetState()
        {
            Log.Debug("Resetting state");

            _transaction.Guid = Guid.Empty;
            _transaction.Amount = 0;
            _transaction.NewAccountBalance = 0;

            Persist();
        }

        private void Persist()
        {
            var block = GetStorageBlock();

            using (var transaction = block.StartTransaction())
            {
                transaction["TransactionGuid"] = _transaction.Guid.ToByteArray();
                transaction["Amount"] = _transaction.Amount;
                transaction["NewAccountBalance"] = _transaction.NewAccountBalance;

                transaction.Commit();
            }
        }

        private void Retrieve()
        {
            _transaction = new Transaction();

            var block = GetStorageBlock();
            _transaction.Guid = new Guid((byte[])block["TransactionGuid"]);
            _transaction.Amount = (long)block["Amount"];
            _transaction.NewAccountBalance = (long)block["NewAccountBalance"];
        }

        private void ReceiveEvent(IEvent data)
        {
            if (typeof(DebugNoteEvent) == data.GetType())
            {
                var debugNoteEvent = (DebugNoteEvent)data;

                if (debugNoteEvent.Denomination == 0)
                {
                    _restrictDebugCreditsIn = !_restrictDebugCreditsIn;
                    Log.DebugFormat(
                        "Restrict debug credits in to configured max credits in limit is {0}",
                        _restrictDebugCreditsIn ? "on" : "off");
                    return;
                }
            }

            if (_noteAcceptorEnabled == false && !_showModeActive)
            {
                var noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                if (noteAcceptor != null)
                {
                    var reasonDisabled = noteAcceptor.ReasonDisabled;

                    if (!_restrictDebugCreditsIn)
                    {
                        if (reasonDisabled != DisabledReasons.Configuration)
                        {
                            Log.Debug(
                                $"Note acceptor is disabled for reason {reasonDisabled}, unable to process debug credits.");
                            return;
                        }
                    }
                    else
                    {
                        Log.Debug(
                            $"Note acceptor is disabled for reason {reasonDisabled}, unable to process debug credits.");
                        return;
                    }
                }
            }

            if (ServiceManager.GetInstance().GetService<ISystemDisableManager>().IsDisabled)
            {
                Log.Debug("System is currently disabled, unable to process debug credits.");
                return;
            }

            // If we have a transaction guid, ignore this event.
            if (Guid.Empty != _transaction.Guid)
            {
                Log.Debug($"Transaction guid unavailable, ignoring event {data.GetType()}.");
                return;
            }

            HandleEvent(data as dynamic);
        }

        private static bool CheckMaxCreditMeter(long amount)
        {
            var bank = ServiceManager.GetInstance().GetService<IBank>();

            return bank.QueryBalance() + amount < bank.Limit;
        }

        private void UpdateShowModeAccountBalance()
        {
            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            if (!gameProvider.GetEnabledGames().Any())
            {
                Log.Info("Can't update show mode account balance when no game is enabled.");
                return;
            }

            var wagerMatchEnabled = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(GamingConstants.ShowProgramEnableResetCredits, true);
            if (!wagerMatchEnabled)
            {
                return;
            }

            var balance = ServiceManager.GetInstance().GetService<IBank>().QueryBalance();

            var bank = ServiceManager.GetInstance().GetService<IBank>();

            // set to half of limit, rounded to nearest $100
            var limit = (long)Math.Round((double)bank.Limit / 2 / 100M.DollarsToMillicents()) * 100M.DollarsToMillicents();
            limit = Math.Min(limit, _showModeMaxBankReset);

            if (balance < limit)
            {
                if (UpdateBalance(limit - balance, AccountType.Cashable))
                {
                    UpdateMeters(AccountType.Cashable, limit - balance);
                }
            }
        }

        private void ShowModeHandleEvent(HandpayKeyOffPendingEvent _)
        {
            if (_showModeLockupTimer == null)
            {
                _showModeLockupTimer = new Timer(
                    ResetLockup,
                    null,
                    ShowModeHandpayLockupDuration,
                    System.Threading.Timeout.Infinite);
            }
        }

        private void ResetLockup(object _)
        {
            _showModeLockupTimer?.Dispose();
            _showModeLockupTimer = null;

            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            eventBus.Publish(new DownEvent((int)ButtonLogicalId.Button30));
        }

        private static bool CheckLaundry(long amount)
        {
            if (!ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.CheckLaundryLimit, false))
            {
                return true;
            }

            var limit = ServiceManager.GetInstance().GetService<IPropertiesManager>().GetValue(
                AccountingConstants.MaxTenderInLimit,
                AccountingConstants.DefaultMaxTenderInLimit);

            var cashInLaundry = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.CashInLaundry, 0L);
            var voucherInLaundry = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.VoucherInLaundry, 0L);

            var amountToCheck = voucherInLaundry + cashInLaundry + amount;

            // If the current value plus amount is less than or equal the limit, then we accept the document
            var underLimit = amountToCheck <= limit;
            if (!underLimit)
            {
                Log.Warn($"Check laundry amount {amountToCheck} exceeds MaxTenderInLimit {limit}");
            }

            return underLimit;
        }

        private static void UpdateLaundryLimit(BillTransaction transaction)
        {
            if (!ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.CheckLaundryLimit, false))
            {
                return;
            }

            var cashInLaundry = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.CashInLaundry, 0L);

            ServiceManager.GetInstance().GetService<IPropertiesManager>().SetProperty(
                AccountingConstants.CashInLaundry,
                cashInLaundry + transaction.Amount);
        }

        private static void ResetLaundryLimit()
        {
            var bank = ServiceManager.GetInstance().GetService<IBank>();

            var currentBalance = bank.QueryBalance();

            if (currentBalance > 0)
            {
                return;
            }

            if (!ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(AccountingConstants.CheckLaundryLimit, false))
            {
                return;
            }

            ServiceManager.GetInstance().GetService<IPropertiesManager>().SetProperty(
                AccountingConstants.CashInLaundry,
                0L);

            ServiceManager.GetInstance().GetService<IPropertiesManager>().SetProperty(
                AccountingConstants.VoucherInLaundry,
                0L);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Transaction
        {
            /// <summary>
            ///     The unique transaction guid, as assigned by the transaction coordinator.
            /// </summary>
            public Guid Guid;

            /// <summary>
            ///     The value of the escrowed currency.
            /// </summary>
            public long Amount;

            /// <summary>
            ///     The expected cashable account balance after depositing the escrowed currency.
            /// </summary>
            public long NewAccountBalance;
        }
    }
}