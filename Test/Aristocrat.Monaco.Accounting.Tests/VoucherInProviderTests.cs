using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Accounting.Contracts;
using Aristocrat.Monaco.Application.Contracts;
using Aristocrat.Monaco.Application.Contracts.Extensions;
using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
using Aristocrat.Monaco.Hardware.Contracts.Persistence;
using Aristocrat.Monaco.Kernel;
using Aristocrat.Monaco.Kernel.Contracts;
using Aristocrat.Monaco.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Addins;
using Moq;

namespace Aristocrat.Monaco.Accounting.Tests
{
    [TestClass]
    public class VoucherInProviderTests
    {
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
        private Mock<IVoucherValidator> _validator;
        private Mock<IValidationProvider> _validationProvider;
        private Mock<IDisposable> _disposable;
        private Mock<IScopedTransaction> _scopedTransaction;

        private VoucherInProvider _target;
        private const int BarcodeLength = 18;
        private const int RequestTimeoutLength = 1000; // It's in milliseconds
        private static readonly Guid RequestorId = new Guid("{B38E80F0-1B82-4571-AEE6-75ADEB9A13BB}");

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _validator = MoqServiceManager.CreateAndAddService<IVoucherValidator>(MockBehavior.Default);
            _validationProvider = MoqServiceManager.CreateAndAddService<IValidationProvider>(MockBehavior.Default);
            _noteAcceptor = new Mock<INoteAcceptor>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _storageManager = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _iidProvider = new Mock<IIdProvider>(MockBehavior.Default);
            _messageDisplay = new Mock<IMessageDisplay>(MockBehavior.Default);
            _scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);
            _validationProvider.Setup(v => v.GetVoucherValidator(false)).Returns(_validator.Object);
            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();
            MockLocalization.Setup(MockBehavior.Default);
        }

        private VoucherInProvider GetTarget()
        {
            VoucherInProvider target = new VoucherInProvider(
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
                _validationProvider.Object
            );
            return target;
        }

        [TestMethod]
        public void BankBalanceExceedTest()
        {
            var deviceId = 1;
            //var transactionDateTime = DateTime.MaxValue;
            var accountType = AccountType.Cashable;
            var barcode = "1234567890ABCDEFGH";

            _properties.Setup(x => x.GetProperty(ApplicationConstants.NoteAcceptorDiagnosticsKey, It.IsAny<bool>()))
                .Returns(false);
            _properties.Setup(x => x.GetProperty(PropertyKey.VoucherIn, It.IsAny<bool>()))
                .Returns(true);
            _properties.Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(false);
            _properties.Setup(x => x.GetProperty(AccountingConstants.CheckLaundryLimit, It.IsAny<bool>()))
                .Returns(false);
            _properties.Setup(x => x.GetProperty(AccountingConstants.VoucherInLimit, It.IsAny<long>()))
                .Returns(119900L);
            _properties.Setup(x => x.GetProperty(AccountingConstants.IsVoucherRedemptionTimedOut, It.IsAny<bool>()))
                .Returns(false);

            const long currentBankBalance = 100;
            const long bankLimit = 1000;
            _bank.Setup(x => x.Limit).Returns(bankLimit);
            _bank.Setup(x => x.QueryBalance()).Returns(currentBankBalance);
            _bank.Object.Deposit(accountType, currentBankBalance, new Guid("{1241B14C-C962-4DBA-B080-260412CA7435}"));

            _validator.Setup(m => m.CanValidateVouchersIn).Returns(true);

            _storageManager.Setup(x => x.ScopedTransaction()).Returns(_scopedTransaction.Object);
            _scopedTransaction.Setup(x => x.Complete());

            Guid transactionId = new Guid("{0241B14C-C962-4DBA-B080-260412CA7435}");
            _transactionCoordinator.Setup(m => m.RequestTransaction(RequestorId, RequestTimeoutLength, TransactionType.Write))
                .Returns(transactionId);

            IReadOnlyCollection<VoucherInTransaction> emptyTrans = new ReadOnlyCollection<VoucherInTransaction>(new[] { new VoucherInTransaction()});
            _transactionHistory.Setup(m => m.RecallTransactions<VoucherInTransaction>())
                .Returns(emptyTrans);

            _iidProvider.Setup(m => m.GetNextLogSequence<VoucherBaseTransaction>()).Returns(1);

            VoucherInTransaction voucherInTransaction = null;
            _validator.Setup(m => m.RedeemVoucher(It.IsAny<VoucherInTransaction>()))
                .Returns(Task.FromResult<VoucherAmount>(null))
                .Callback((VoucherInTransaction t) =>
                    {
                        t.Amount = 1000;
                        voucherInTransaction = t;
                    }
                );
            
            _target = GetTarget();

            var voucherEscrowedEvent = new VoucherEscrowedEvent(deviceId, barcode);
           
            Func<VoucherEscrowedEvent, CancellationToken, Task> onFunc = null;
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Func<VoucherEscrowedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<VoucherEscrowedEvent, CancellationToken, Task>>(
                    (tar, func) =>
                    {
                        onFunc = func;
                    });

            _noteAcceptor.Setup(m => m.AcceptTicket()).Returns(Task.FromResult(true));

            // Setup Accept if the test does not call Reject
            Mock<IMeter> meterMock = new Mock<IMeter>(MockBehavior.Default);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.VoucherInCashableAmount)).Returns(meterMock.Object);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.VoucherInCashableCount)).Returns(meterMock.Object);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.VoucherInCashablePromoAmount)).Returns(meterMock.Object);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.VoucherInCashablePromoCount)).Returns(meterMock.Object);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.VoucherInNonCashableAmount)).Returns(meterMock.Object);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.VoucherInNonCashableCount)).Returns(meterMock.Object);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.VouchersRejectedCount)).Returns(meterMock.Object);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.DocumentsAcceptedCount)).Returns(meterMock.Object);

            _target.Initialize();

            Assert.IsNotNull(onFunc);
            Assert.IsTrue(onFunc(voucherEscrowedEvent, new CancellationToken(false)).Wait(1000));
            Assert.AreEqual(VoucherState.Rejected, voucherInTransaction.State);
        }
    }
}
