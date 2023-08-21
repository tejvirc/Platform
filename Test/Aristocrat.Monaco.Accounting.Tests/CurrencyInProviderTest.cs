namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CurrencyInProviderTest
    {
        private const int RequestTimeoutLength = 1000; // It's in milliseconds
        private static readonly Guid RequestorId = new Guid("{8C475AA8-687F-47CE-9056-39157FA9D6A8}");
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IBank> _bank;
        private Mock<ITransactionCoordinator> _transactionCoordinator;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IEventBus> _eventBus;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPropertiesManager> _properties;
        private Mock<IPersistentStorageManager> _storageManager;
        private Mock<IIdProvider> _iidProvider;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IValidationProvider> _validationProvider;
        private Mock<IScopedTransaction> _scopedTransaction;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;

        private Mock<IDisposable> _disposable;

        private CurrencyInProvider _target;
        private ITransaction _result;
        private Func<CurrencyEscrowedEvent, CancellationToken, Task> _onCurrencyEscrowedEvent;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _validationProvider = MoqServiceManager.CreateAndAddService<IValidationProvider>(MockBehavior.Default);
            _noteAcceptor = new Mock<INoteAcceptor>(MockBehavior.Default);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Default);
            _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
            _properties = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _storageManager = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _iidProvider = new Mock<IIdProvider>(MockBehavior.Default);
            _messageDisplay = new Mock<IMessageDisplay>(MockBehavior.Default);
            _scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);
            _validationProvider.Setup(v => v.GetCurrencyValidator(false)).Returns((ICurrencyValidator)null);
            _persistentStorageAccessor = new Mock<IPersistentStorageAccessor>();

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            MockLocalization.Setup(MockBehavior.Default);

            _transactionHistory.Setup(h => h.UpdateTransaction(It.IsAny<ITransaction>()))
                .Callback<ITransaction>(r => _result = r);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        /// <summary>
        ///     Tests for Bank Balance < MaxCreditLimit
        /// </summary>
        [TestMethod]
        public async Task BankBalanceLessThanMaxCreditLmitAddNoteAcceptedTest()
        {
            const long currentBankBalance = 950;
            SetupCurrencyInProvider(currentBankBalance);

            _target.Initialize();

            Assert.IsNotNull(_onCurrencyEscrowedEvent);

            var currencyEscrowedEvent = GetCurrencyEvent(1);
            await _onCurrencyEscrowedEvent(currencyEscrowedEvent, new CancellationToken(false));
            VerifyCurrencyEscrowedEvent(CurrencyState.Accepted, CurrencyInExceptionCode.None, 1);
        }

        /// <summary>
        ///     Tests for Bank Balance > MaxCreditLimit
        /// </summary>
        [TestMethod]
        public async Task BankBalanceGreaterThanMaxCreditLmitAddNoteRejectedTest()
        {
            const long currentBankBalance = 999;
            SetupCurrencyInProvider(currentBankBalance);

            _target.Initialize();

            Assert.IsNotNull(_onCurrencyEscrowedEvent);

            var currencyEscrowedEvent = GetCurrencyEvent(5);
            await _onCurrencyEscrowedEvent(currencyEscrowedEvent, new CancellationToken(false));
            VerifyCurrencyEscrowedEvent(CurrencyState.Rejected, CurrencyInExceptionCode.CreditLimitExceeded, 1);
        }

        /// <summary>
        ///     Tests for Bank Balance = MaxCreditLimit
        /// </summary>
        [TestMethod]
        public async Task BankBalanceLessThanMaxCreditLmitAddNoteAcceptedForMaxCreditLimitTest()
        {
            const long currentBankBalance = 999;
            SetupCurrencyInProvider(currentBankBalance);

            _target.Initialize();

            Assert.IsNotNull(_onCurrencyEscrowedEvent);

            var currencyEscrowedEvent = GetCurrencyEvent(1);
            await _onCurrencyEscrowedEvent(currencyEscrowedEvent, new CancellationToken(false));
            VerifyCurrencyEscrowedEvent(CurrencyState.Accepted, CurrencyInExceptionCode.None, 1);
        }

        private static CurrencyEscrowedEvent GetCurrencyEvent(int noteValue)
        {
            var note = new Note { NoteId = 1, Value = noteValue, ISOCurrencySymbol = "USD", Version = 1 };
            var currencyEscrowedEvent = new CurrencyEscrowedEvent(note);
            return currencyEscrowedEvent;
        }

        private CurrencyInProvider GetTarget()
        {
            var target = new CurrencyInProvider(
                _noteAcceptor.Object,
                _bank.Object,
                _transactionCoordinator.Object,
                _transactionHistory.Object,
                _eventBus.Object,
                _meterManager.Object,
                _properties.Object,
                _storageManager.Object,
                _iidProvider.Object,
                _messageDisplay.Object,
                _validationProvider.Object);

            return target;
        }

        private void VerifyCurrencyEscrowedEvent(
            CurrencyState expectedState,
            CurrencyInExceptionCode expectedExceptionCode,
            int expectedNoOfTimes)
        {
            Assert.AreEqual(expectedState, ((BillTransaction)_result).State);
            Assert.AreEqual((int)expectedExceptionCode, ((BillTransaction)_result).Exception);
            _messageDisplay.Verify(
                x => x.DisplayMessage(It.IsAny<DisplayableMessage>()),
                Times.Exactly(expectedNoOfTimes));
        }

        private void SetupCurrencyInProvider(long currentBankBalance)
        {
            SetupStorage();

            SetupProperties();

            const long bankLimit = 1000;
            SetupBank(currentBankBalance, bankLimit);

            SetupTransaction();

            _target = GetTarget();

            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Func<CurrencyEscrowedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<CurrencyEscrowedEvent, CancellationToken, Task>>(
                    (tar, func) => { _onCurrencyEscrowedEvent = func; });

            _iidProvider.Setup(m => m.GetNextLogSequence<BillTransaction>()).Returns(1);

            SetupNoteAcceptor();

            var meterMock = new Mock<IMeter>(MockBehavior.Default);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(meterMock.Object);
        }

        private void SetupBank(long currentBankBalance, long bankLimit)
        {
            var accountType = AccountType.Cashable;
            _bank.Setup(x => x.Limit).Returns(bankLimit);
            _bank.Setup(x => x.QueryBalance()).Returns(currentBankBalance);
            _bank.Object.Deposit(accountType, currentBankBalance, new Guid("{1241B14C-C962-4DBA-B080-260412CA7435}"));
        }

        private void SetupNoteAcceptor()
        {
            _noteAcceptor.Setup(m => m.AcceptTicket()).Returns(Task.FromResult(true));
            _noteAcceptor.Setup(m => m.Return()).Returns(Task.FromResult(true));
            _noteAcceptor.Setup(m => m.DenomIsValid(It.IsAny<int>())).Returns(true);
            _noteAcceptor.Setup(m => m.AcceptNote()).Returns(Task.FromResult(true));
        }

        private void SetupTransaction()
        {
            _scopedTransaction.Setup(x => x.Complete());

            var transactionId = new Guid("{0241B14C-C962-4DBA-B080-260412CA7435}");
            _transactionCoordinator
                .Setup(m => m.RequestTransaction(RequestorId, RequestTimeoutLength, TransactionType.Write))
                .Returns(transactionId);

            IReadOnlyCollection<BillTransaction> emptyTrans =
                new ReadOnlyCollection<BillTransaction>(new[] { new BillTransaction() });
            _transactionHistory.Setup(m => m.RecallTransactions<BillTransaction>()).Returns(emptyTrans);
        }

        private void SetupStorage()
        {
            _storageManager.Setup(x => x.BlockExists(It.IsAny<string>())).Returns(true);
            _storageManager.Setup(x => x.GetBlock(It.IsAny<string>())).Returns(_persistentStorageAccessor.Object);
            _storageManager.Setup(x => x.ScopedTransaction()).Returns(_scopedTransaction.Object);
            _persistentStorageAccessor.Setup(x => x.StartTransaction())
                .Returns(new Mock<IPersistentStorageTransaction>().Object);
        }

        private void SetupProperties()
        {
            _properties.Setup(x => x.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1d)).Returns(1d);
            _properties.Setup(x => x.GetProperty(ApplicationConstants.CurrencyId, string.Empty)).Returns(string.Empty);
            _properties.Setup(x => x.GetProperty(ApplicationConstants.NoteAcceptorDiagnosticsKey, It.IsAny<bool>()))
                .Returns(false);
            _properties.Setup(x => x.GetProperty(PropertyKey.VoucherIn, It.IsAny<bool>())).Returns(true);
            _properties.Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(false);
            _properties.Setup(x => x.GetProperty(AccountingConstants.CheckLaundryLimit, It.IsAny<bool>()))
                .Returns(false);
            _properties.Setup(x => x.GetProperty(AccountingConstants.VoucherInLimit, It.IsAny<long>()))
                .Returns(119900L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.IsVoucherRedemptionTimedOut, It.IsAny<bool>()))
                .Returns(false);
            _properties.Setup(
                    x =>
                        x.GetProperty(AccountingConstants.ShowMessageWhenCreditLimitReached, It.IsAny<bool>()))
                .Returns(true);
        }
    }
}