﻿namespace Aristocrat.Monaco.Accounting.CoinAcceptor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.CoinAcceptor;
    using Contracts.Transactions;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.PWM;
    using Kernel;
    using Localization.Properties;
    using log4net;
    
    [CLSCompliant(false)]
    public class CoinProvider : IService, IDisposable
    {
        private const int DIVERT_TOLERANCE = 5;
        private const int DeviceId = 1;
        private int _diverterErrors = 0;

        private const int RequestTimeoutLength = 1000; // It's in milliseconds

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Guid RequestorId = new Guid("{EBB8B24C-771F-474A-8315-4F25DDBDBEA3}");

        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly ITransactionCoordinator _coordinator;
        private readonly IMeterManager _meters;
        private readonly IPersistentStorageManager _storage;
        private readonly IMessageDisplay _messageDisplay;
        private readonly ITransactionHistory _transactions;
        private readonly IIdProvider _idProvider;
        private ICoinAcceptor _coinAcceptorService;
        private IPropertiesManager _propertiesManager;


        public CoinProvider()
            : this(
                ServiceManager.GetInstance().TryGetService<ICoinAcceptor>(),
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionCoordinator>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IIdProvider>(),
                ServiceManager.GetInstance().GetService<IMessageDisplay>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        [CLSCompliant(false)]
        public CoinProvider(
            ICoinAcceptor coinAcceptor,
            IBank bank,
            ITransactionCoordinator coordinator,
            ITransactionHistory transactionHistory,
            IEventBus bus,
            IMeterManager meters,
            IPersistentStorageManager storage,
            IIdProvider idProvider,
            IMessageDisplay messageDisplay,
            IPropertiesManager propertiesManager)
        {
            _coinAcceptorService = coinAcceptor ?? throw new ArgumentNullException(nameof(coinAcceptor));
            _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        public void Dispose()
        {
            _bus.UnsubscribeAll(this);
        }

        public string Name => nameof(CoinProvider);

        public ICollection<Type> ServiceTypes => new[] { typeof(CoinProvider) };

        public void Initialize()
        {
            _bus.Subscribe<CoinInEvent>(this, Handle);
            _bus.Subscribe<CoinToCashboxInEvent>(this, Handle);
            _bus.Subscribe<CoinToHopperInEvent>(this, Handle);
            _bus.Subscribe<CoinToCashboxInsteadOfHopperEvent>(this, Handle);
            _bus.Subscribe<CoinToHopperInsteadOfCashboxEvent>(this, Handle);
            _bus.Subscribe<TransferOutCompletedEvent>(this, _ => _coinAcceptorService?.DivertMechanismOnOff());
        }

        private bool Commit(
            CoinTransaction transaction,
            Guid transactionId, ICoin coin)
        {
            Commit(transactionId, transaction, coin);
            return true;
        }

        private void Handle(CoinInEvent evt)
        {
            if (CoinAcceptorDiagnosticMode())
            {
                return;
            }
            var transaction = CreateCoinTransaction(DeviceId);

            Guid transactionId;
            using (var scope = _storage.ScopedTransaction())
            {
                transactionId = _coordinator.RequestTransaction(
                    RequestorId,
                    RequestTimeoutLength,
                    TransactionType.Write);

                if (transactionId == Guid.Empty)
                {
                    Logger.Error("Coin In Event : Failed to acquire a transaction.");

                    return;
                }

                // Unique log sequence number assigned by the EGM; a series that strictly increases by 1 (one) starting at 1 (one).
                transaction.LogSequence = _idProvider.GetNextLogSequence<CoinTransaction>();
                _transactions.AddTransaction(transaction);

                scope.Complete();
            }

            _bus.Publish(new CoinInStartedEvent(evt.Coin));

            transaction.Exception = (int)CurrencyInExceptionCode.None;

            if (!Commit(transaction, transactionId, evt.Coin))
            {
                return;
            }

            Logger.Info($"Accepted coin: {evt}");

            DisplayMessage((CurrencyInExceptionCode)transaction.Exception, evt.Coin.Value);

            _coinAcceptorService?.DivertMechanismOnOff();
        }

        private void DisplayMessage(CurrencyInExceptionCode exceptionCode, long acceptedAmount = 0)
        {
            _messageDisplay.DisplayMessage(
                new DisplayableMessage(
                    () => Localizer.For(CultureFor.Player).FormatString(ResourceKeys.CoinAccepted) +
                          " " + acceptedAmount.MillicentsToDollars().FormattedCurrencyString(),
                    DisplayableMessageClassification.Informative,
                    DisplayableMessagePriority.Normal,
                    typeof(CoinInCompletedEvent)));
        }

        private CoinTransaction CreateCoinTransaction(int deviceId) =>
            CreateCoinTransaction(
                deviceId,
                CoinInDetails.None,
                CurrencyInExceptionCode.None);

        private CoinTransaction CreateCoinTransaction(
            int deviceId,
            CoinInDetails detailsCode,
            CurrencyInExceptionCode exceptionCode)
        {
            return new CoinTransaction(
                deviceId,
                DateTime.UtcNow,
                (int)detailsCode,
                (int)exceptionCode);
        }

        private void Commit(Guid transactionId, CoinTransaction transaction, ICoin coin)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                transaction.Accepted = DateTime.UtcNow;

                _transactions.UpdateTransaction(transaction);

                _bank.Deposit(transaction.TypeOfAccount, coin.Value, transactionId);

                _meters.GetMeter(AccountingMeters.TrueCoinInCount).Increment(1);

                _coordinator.ReleaseTransaction(transactionId);

                scope.Complete();
            }

            _bus.Publish(new CoinInCompletedEvent(coin, transaction));
        }

        private void Handle(CoinToCashboxInEvent evt)
        {
            if (CoinAcceptorDiagnosticMode())
            {
                return;
            }
            var transaction = _transactions.GetLast<CoinTransaction>();
            using (var scope = _storage.ScopedTransaction())
            {
                transaction.Details = (int)CoinInDetails.CoinToCashBox;

                _meters.GetMeter(AccountingMeters.CoinToCashBoxCount).Increment(1);

                _transactions.UpdateTransaction(transaction);

                scope.Complete();
            }

            _diverterErrors = 0;
        }

        private void Handle(CoinToHopperInEvent evt)
        {
            if (CoinAcceptorDiagnosticMode())
            {
                return;
            }
            var transaction = _transactions.GetLast<CoinTransaction>();
            using (var scope = _storage.ScopedTransaction())
            {
                transaction.Details = (int)CoinInDetails.CoinToHopper;
                _meters.GetMeter(AccountingMeters.CoinToHopperCount).Increment(1);
                _transactions.UpdateTransaction(transaction);

                scope.Complete();
            }

            _diverterErrors = 0;
        }

        private void Handle(CoinToCashboxInsteadOfHopperEvent evt)
        {
            if (CoinAcceptorDiagnosticMode())
            {
                return;
            }
            var transaction = _transactions.GetLast<CoinTransaction>();
            using (var scope = _storage.ScopedTransaction())
            {
                transaction.Details = (int)CoinInDetails.CoinToCashBoxInsteadHopper;
                _meters.GetMeter(AccountingMeters.CoinToCashBoxCount).Increment(1);
                _meters.GetMeter(AccountingMeters.CoinToCashBoxInsteadHopperCount).Increment(1);
                _transactions.UpdateTransaction(transaction);

                scope.Complete();
            }

            _diverterErrors++;
            CheckDivertError();
        }

        private void Handle(CoinToHopperInsteadOfCashboxEvent evt)
        {
            if (CoinAcceptorDiagnosticMode())
            {
                return;
            }
            var transaction = _transactions.GetLast<CoinTransaction>();
            using (var scope = _storage.ScopedTransaction())
            {
                transaction.Details = (int)CoinInDetails.CoinToHopperInsteadCashBox;
                _meters.GetMeter(AccountingMeters.CoinToHopperCount).Increment(1);
                _meters.GetMeter(AccountingMeters.CoinToHopperInsteadCashBoxCount).Increment(1);
                _transactions.UpdateTransaction(transaction);

                scope.Complete();
            }

            _diverterErrors++;
            CheckDivertError();
        }

        private void CheckDivertError()
        {
            if (_diverterErrors >= DIVERT_TOLERANCE)
            {
                _diverterErrors = 0;
                _bus.Publish(new HardwareFaultEvent(CoinFaultTypes.Divert));
            }
        }

        private bool CoinAcceptorDiagnosticMode()
        {
            return _propertiesManager.GetValue(HardwareConstants.CoinAcceptorDiagnosticMode, false);
        }
    }
}
