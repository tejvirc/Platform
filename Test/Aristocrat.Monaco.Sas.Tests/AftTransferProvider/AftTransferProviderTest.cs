namespace Aristocrat.Monaco.Sas.Tests.AftTransferProvider
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.Monaco.Sas.Storage;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.Metering;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.AftTransferProvider;

    /// <summary>
    ///     Contains the unit tests for the AftTransferProvider class
    /// </summary>
    [TestClass]
    public class AftTransferProviderTest
    {
        private AftTransferProvider _target;
        private readonly Mock<IAftLockHandler> _lockHandler = new Mock<IAftLockHandler>(MockBehavior.Default);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Default);
        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
        private readonly Mock<IAftOffTransferProvider> _aftOff = new Mock<IAftOffTransferProvider>(MockBehavior.Default);
        private readonly Mock<IAftOnTransferProvider> _aftOn = new Mock<IAftOnTransferProvider>(MockBehavior.Default);
        private readonly Mock<IAftHistoryBuffer> _historyBuffer = new Mock<IAftHistoryBuffer>(MockBehavior.Default);
        private readonly Mock<ITicketingCoordinator> _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
        private readonly Mock<ITime> _time = new Mock<ITime>(MockBehavior.Default);
        private readonly Mock<ISasBonusCallback> _bonus = new Mock<ISasBonusCallback>(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Default);
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Default);
        private readonly Mock<IStorageDataProvider<AftTransferOptions>> _transferOptionsProvider = new Mock<IStorageDataProvider<AftTransferOptions>>(MockBehavior.Default);
        private readonly Mock<IAftRegistrationProvider> _registrationProvider = new Mock<IAftRegistrationProvider>(MockBehavior.Default);
        private readonly Mock<IMoneyLaunderingMonitor> _laundry = new Mock<IMoneyLaunderingMonitor>(MockBehavior.Default);
        private readonly Mock<IPlayerBank> _playerBank = new Mock<IPlayerBank>(MockBehavior.Default);
        private const ulong TransferLimit = 1000UL;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank.Setup(m => m.Limit).Returns(100L);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                TransferLimit = (long)TransferLimit,
                AftBonusAllowed = true,
                TransferInAllowed = true,
                TransferOutAllowed = true
            });

            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions());
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _target = CreateProvider();
        }

        private AftTransferProvider CreateProvider(
            bool nullLockHandler = false,
            bool nullBank = false,
            bool nullException = false,
            bool nullProperties = false,
            bool nullMeter = false,
            bool nullAftOff = false,
            bool nullAftOn = false,
            bool nullPersistence = false,
            bool nullTicket = false,
            bool nullHistory = false,
            bool nullBonus = false,
            bool nullTime = false,
            bool nullLaundry = false,
            bool nullPlayerBank = false
            )
        {
            return new AftTransferProvider(
                nullLockHandler ? null : _lockHandler.Object,
                nullBank ? null : _bank.Object,
                nullException ? null : _exceptionHandler.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullMeter ? null : _meterManager.Object,
                nullAftOff ? null : _aftOff.Object,
                nullAftOn ? null : _aftOn.Object,
                nullTicket ? null : _ticketingCoordinator.Object,
                _registrationProvider.Object,
                nullHistory ? null : _historyBuffer.Object,
                nullPersistence ? null : _unitOfWorkFactory.Object,
                _transferOptionsProvider.Object,
                nullBonus ? null : _bonus.Object,
                nullTime ? null : _time.Object,
                nullLaundry ? null : _laundry.Object,
                nullPlayerBank ? null : _playerBank.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, true, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, true, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, true, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, true, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, true, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, true, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullLockHandler,
            bool nullBank,
            bool nullException,
            bool nullProperties,
            bool nullMeter,
            bool nullAftOff,
            bool nullAftOn,
            bool nullPersistence,
            bool nullTicket,
            bool nullHistory,
            bool nullBonus,
            bool nullTime,
            bool nullLaundy,
            bool nullPlayerBank
            )
        {
            CreateProvider(
                nullLockHandler,
                nullBank,
                nullException,
                nullProperties,
                nullMeter,
                nullAftOff,
                nullAftOn,
                nullPersistence,
                nullTicket,
                nullHistory,
                nullBonus,
                nullTime,
                nullLaundy,
                nullPlayerBank);
        }

        [TestMethod]
        public void CurrentTransferInitializedTest()
        {
            Assert.IsNotNull(_target.CurrentTransfer);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, _target.CurrentTransfer.ReceiptStatus);
            Assert.AreEqual(AftTransferStatusCode.NoTransferInfoAvailable, _target.CurrentTransfer.TransferStatus);
        }

        [TestMethod]
        public void RecoveryFinishedTransactionNotAcknowledgedTest()
        {
            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = 123,
                TransactionDateTime = DateTime.Now,
                RegistrationKey = new byte[] { },
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferType = AftTransferType.GameToHostInHouse
            };

            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransfer = StorageHelpers.Serialize(response),
                IsTransferAcknowledgedByHost = false
            });

            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.AftTransferComplete)))
                .Verifiable();

            AddMeterMock(100, AccountingMeters.WatOffCashableAmount);
            AddMeterMock(200, AccountingMeters.WatOffNonCashableAmount);
            AddMeterMock(300, AccountingMeters.WatOffCashablePromoAmount);

            _target = CreateProvider();

            _target.OnSasInitialized();
            _exceptionHandler.Verify();
        }

        [TestMethod]
        public void RecoveryPendingOutFailedTransactionNotAcknowledgedTest()
        {
            const string transactionId = "TransId";
            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                TransactionId = transactionId,
                AssetNumber = 123,
                TransactionDateTime = DateTime.Now,
                RegistrationKey = new byte[] { },
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferType = AftTransferType.GameToHostInHouse
            };

            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransfer = StorageHelpers.Serialize(response),
                IsTransferAcknowledgedByHost = false
            });

            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.AftTransferComplete)))
                .Verifiable();
            _aftOff.Setup(x => x.Recover(transactionId)).Returns(false).Verifiable();
            _time.Setup(x => x.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);

            AddMeterMock(100, AccountingMeters.WatOffCashableAmount);
            AddMeterMock(200, AccountingMeters.WatOffNonCashableAmount);
            AddMeterMock(300, AccountingMeters.WatOffCashablePromoAmount);

            _target = CreateProvider();

            _target.OnSasInitialized();

            Assert.AreEqual((ulong)0, _target.CurrentTransfer.CashableAmount);
            Assert.AreEqual((ulong)0, _target.CurrentTransfer.NonRestrictedAmount);
            Assert.AreEqual((ulong)0, _target.CurrentTransfer.RestrictedAmount);
            Assert.AreEqual(DateTime.MaxValue, _target.CurrentTransfer.TransactionDateTime);
            Assert.AreEqual(AftTransferStatusCode.UnexpectedError, _target.CurrentTransfer.TransferStatus);
            _exceptionHandler.Verify();
            _aftOff.Verify();
        }

        [TestMethod]
        public void RecoveryPendingOutTransactionNotAcknowledgedTest()
        {
            const string transactionId = "TransId";
            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                TransactionId = transactionId,
                AssetNumber = 123,
                TransactionDateTime = DateTime.Now,
                RegistrationKey = new byte[] { },
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferType = AftTransferType.GameToHostInHouse
            };

            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransfer = StorageHelpers.Serialize(response),
                IsTransferAcknowledgedByHost = false
            });

            _aftOff.Setup(x => x.Recover(transactionId)).Returns(true).Verifiable();

            AddMeterMock(100, AccountingMeters.WatOffCashableAmount);
            AddMeterMock(200, AccountingMeters.WatOffNonCashableAmount);
            AddMeterMock(300, AccountingMeters.WatOffCashablePromoAmount);

            _target = CreateProvider();

            _target.OnSasInitialized();
            _aftOff.Verify();
        }

        [TestMethod]
        public void RecoveryPendingInTransactionNotAcknowledgedTest()
        {
            const string transactionId = "TransId";
            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                TransactionId = transactionId,
                AssetNumber = 123,
                TransactionDateTime = DateTime.Now,
                RegistrationKey = new byte[] { },
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferType = AftTransferType.HostToGameInHouse
            };

            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransfer = StorageHelpers.Serialize(response),
                IsTransferAcknowledgedByHost = false
            });

            _aftOn.Setup(x => x.Recover(transactionId)).Returns(true).Verifiable();

            AddMeterMock(100, AccountingMeters.WatOnCashableAmount);
            AddMeterMock(200, AccountingMeters.WatOnNonCashableAmount);
            AddMeterMock(300, AccountingMeters.WatOnCashablePromoAmount);

            _target = CreateProvider();

            _target.OnSasInitialized();
            _aftOn.Verify();
        }

        [TestMethod]
        public void AssetNumberTest()
        {
            uint assetNumber = 123;
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);

            Assert.AreEqual(assetNumber, _target.AssetNumber);
        }

        [TestMethod]
        public void AssetNumberInvalidTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns((uint)0);

            Assert.AreEqual((uint)0, _target.AssetNumber);
        }

        [TestMethod]
        public void RegistrationKeyTest()
        {
            var registration = new byte[20];
            _registrationProvider.Setup(x => x.AftRegistrationKey).Returns(new byte[20]);
            CollectionAssert.AreEqual(registration, _target.RegistrationKey);
        }

        [TestMethod]
        public void IsLockedTest()
        {
            _lockHandler.Setup(m => m.LockStatus).Returns(AftGameLockStatus.GameLocked);

            Assert.IsTrue(_target.IsLocked);

            _lockHandler.Setup(m => m.LockStatus).Returns(AftGameLockStatus.GameNotLocked);

            Assert.IsFalse(_target.IsLocked);
        }

        [TestMethod]
        public void IsRegistrationKeyAllZerosTest()
        {
            _target.CurrentTransfer = new AftResponseData { RegistrationKey = new byte[] { 0x01 } };

            Assert.IsFalse(_target.IsRegistrationKeyAllZeros);

            _target.CurrentTransfer = new AftResponseData();

            Assert.IsTrue(_target.IsRegistrationKeyAllZeros);
        }

        [TestMethod]
        public void PartialTransfersAllowedTest()
        {
            _target.CurrentTransfer = new AftResponseData { TransferCode = AftTransferCode.TransferRequestFullTransferOnly };

            Assert.IsFalse(_target.PartialTransfersAllowed);

            _target.CurrentTransfer = new AftResponseData { TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed };
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { PartialTransferAllowed = false });

            Assert.IsFalse(_target.PartialTransfersAllowed);

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { PartialTransferAllowed = true });

            Assert.IsTrue(_target.PartialTransfersAllowed);
        }

        [TestMethod]
        public void FullTransferRequestedTest()
        {
            _target.CurrentTransfer = new AftResponseData { TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed };

            Assert.IsFalse(_target.FullTransferRequested);

            _target.CurrentTransfer = new AftResponseData { TransferCode = AftTransferCode.TransferRequestFullTransferOnly };

            Assert.IsTrue(_target.FullTransferRequested);
        }

        [TestMethod]
        public void DebitTransferTest()
        {
            _target.CurrentTransfer = new AftResponseData { TransferType = AftTransferType.GameToHostInHouse };

            Assert.IsFalse(_target.DebitTransfer);

            _target.CurrentTransfer = new AftResponseData { TransferType = AftTransferType.HostToGameDebit };

            Assert.IsTrue(_target.DebitTransfer);

            _target.CurrentTransfer = new AftResponseData { TransferType = AftTransferType.HostToGameDebitTicket };

            Assert.IsTrue(_target.DebitTransfer);
        }

        [TestMethod]
        public void MissingRequiredReceiptFieldsTest()
        {
            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferFlags = AftTransferFlags.TransactionReceiptRequested
            };

            // missing Patron Account info
            Assert.IsTrue(_target.MissingRequiredReceiptFields);

            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameDebit,
                TransferFlags = AftTransferFlags.TransactionReceiptRequested
            };

            // missing Debit Account info
            Assert.IsTrue(_target.MissingRequiredReceiptFields);

            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameDebit,
                TransferFlags = AftTransferFlags.None
            };

            // no receipt requested
            Assert.IsFalse(_target.MissingRequiredReceiptFields);

            // receipt for non debit with all data present
            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                ReceiptData = new AftReceiptData { PatronAccount = "1234" },
                TransferFlags = AftTransferFlags.TransactionReceiptRequested
            };

            Assert.IsFalse(_target.MissingRequiredReceiptFields);

            // receipt for debit with all data present
            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameDebitTicket,
                ReceiptData = new AftReceiptData { DebitCardNumber = "1234" },
                TransferFlags = AftTransferFlags.TransactionReceiptRequested
            };

            Assert.IsFalse(_target.MissingRequiredReceiptFields);
        }

        [TestMethod]
        public void PosIdZeroTest()
        {
            _registrationProvider.Setup(x => x.PosId).Returns(0);
            Assert.IsTrue(_target.PosIdZero);

            _registrationProvider.Setup(x => x.PosId).Returns(1);
            Assert.IsFalse(_target.PosIdZero);
        }

        [TestMethod]
        public void TransferFundsRequestTest()
        {
            var data = new AftTransferData { TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed };
            Assert.IsTrue(_target.TransferFundsRequest(data));

            data = new AftTransferData { TransferCode = AftTransferCode.TransferRequestFullTransferOnly };
            Assert.IsTrue(_target.TransferFundsRequest(data));

            data = new AftTransferData { TransferCode = AftTransferCode.CancelTransferRequest };
            Assert.IsFalse(_target.TransferFundsRequest(data));
        }

        [TestMethod]
        public void CheckTransactionIdLengthToShortTest()
        {
            var transactionId = "";
            _historyBuffer.Setup(m => m.GetHistoryEntry(0xFF)).Returns(new AftResponseData { TransactionId = null });

            _target.CheckTransactionId(transactionId);
            Assert.IsFalse(_target.TransactionIdValid);
            Assert.IsTrue(_target.TransactionIdUnique);
        }

        [TestMethod]
        public void CheckTransactionIdLengthToLongTest()
        {
            var transactionId = "123456789012345678901";
            _historyBuffer.Setup(m => m.GetHistoryEntry(0xFF)).Returns(new AftResponseData { TransactionId = "abc" });

            _target.CheckTransactionId(transactionId);
            Assert.IsFalse(_target.TransactionIdValid);
        }

        [TestMethod]
        public void CheckTransactionIdSpecialCharactersTest()
        {
            var transactionId = "123\n";
            _historyBuffer.Setup(m => m.GetHistoryEntry(0xFF)).Returns(new AftResponseData { TransactionId = "abc" });

            _target.CheckTransactionId(transactionId);
            Assert.IsFalse(_target.TransactionIdValid);
        }

        [TestMethod]
        public void CheckTransactionIdLastEntrySameTest()
        {
            var transactionId = "123";
            _historyBuffer.Setup(m => m.GetHistoryEntry(0xFF)).Returns(new AftResponseData { TransactionId = "123" });

            _target.CheckTransactionId(transactionId);
            Assert.IsTrue(_target.TransactionIdValid);
            Assert.IsFalse(_target.TransactionIdUnique);
        }

        [TestMethod]
        public void CheckTransactionIdLastEntryDifferentTest()
        {
            var transactionId = "123";
            _historyBuffer.Setup(m => m.GetHistoryEntry(0xFF)).Returns(new AftResponseData { TransactionId = "abc" });

            _target.CheckTransactionId(transactionId);
            Assert.IsTrue(_target.TransactionIdValid);
            Assert.IsTrue(_target.TransactionIdUnique);
        }

        [TestMethod]
        public void CreateNewTransactionHistoryOnEntryTest()
        {
            const string transferId = "TransId";
            _historyBuffer.Setup(m => m.AddEntry(It.IsAny<AftResponseData>(), It.IsAny<IUnitOfWork>())).Returns(1).Verifiable();
            _historyBuffer.Setup(x => x.CurrentBufferIndex).Returns(1).Verifiable();
            _aftOn.Setup(x => x.AcknowledgeTransfer(transferId));
            _target.TransferAmount = 100;
            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransactionId = transferId,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                TransactionIndex = 1
            };

            _target.CreateNewTransactionHistoryEntry();

            _historyBuffer.Verify();
        }

        [TestMethod]
        public void CreateNewTransactionHistoryOffEntryTest()
        {
            const string transferId = "TransId";
            _historyBuffer.Setup(m => m.AddEntry(It.IsAny<AftResponseData>(), It.IsAny<IUnitOfWork>())).Returns(1).Verifiable();
            _historyBuffer.Setup(x => x.CurrentBufferIndex).Returns(1).Verifiable();
            _aftOff.Setup(x => x.AcknowledgeTransfer(transferId));
            _target.TransferAmount = 100;
            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransactionId = transferId,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                TransactionIndex = 1
            };

            _target.CreateNewTransactionHistoryEntry();

            _historyBuffer.Verify();
        }

        [TestMethod]
        public void AdjustHostCashoutFlagsFailedTransferTest()
        {
            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransferFlags = AftTransferFlags.HostCashOutEnable
            });

            _target.CurrentTransfer = new AftResponseData();
            _target.TransferFailure = true;

            var data = new AftTransferData();
            _target.UpdateHostCashoutFlags(data);

            Assert.AreEqual(AftTransferFlags.HostCashOutEnable, _target.CurrentTransfer.TransferFlags);
        }

        [TestMethod]
        public void AdjustHostCashoutFlagsZeroTest()
        {
            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransferFlags = AftTransferFlags.HostCashOutEnable
            });

            _target.CurrentTransfer = new AftResponseData();
            _target.TransferFailure = false;

            var data = new AftTransferData();
            _target.UpdateHostCashoutFlags(data);

            Assert.AreEqual(AftTransferFlags.HostCashOutEnable, _target.CurrentTransfer.TransferFlags);
        }

        [TestMethod]
        public void AdjustHostCashoutFlagsUpdateTest()
        {
            var waiter = new AutoResetEvent(false);
            _transferOptionsProvider.Setup(x => x.GetData()).Returns(new AftTransferOptions
            {
                CurrentTransferFlags = AftTransferFlags.HostCashOutEnable
            });

            _transferOptionsProvider.Setup(x => x.Save(It.IsAny<AftTransferOptions>()))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());
            _target.CurrentTransfer = new AftResponseData();
            _target.TransferFailure = false;

            var data = new AftTransferData { TransferFlags = AftTransferFlags.HostCashOutEnableControl };

            _target.UpdateHostCashoutFlags(data);
            waiter.WaitOne(100);

            Assert.AreEqual(AftTransferFlags.HostCashOutEnableControl, _target.CurrentTransfer.TransferFlags);
        }

        [DataRow(
            AftTransferStatusCode.FullTransferSuccessful,
            AftTransferCode.TransferRequestFullTransferOnly,
            DisplayName = "Successful Full Transfer")]
        [DataRow(
            AftTransferStatusCode.PartialTransferSuccessful,
            AftTransferCode.TransferRequestPartialTransferAllowed,
            DisplayName = "Successful Partial Transfer")]
        [DataTestMethod]
        public void UpdateFinalAftResponseDataTransferOffTest(AftTransferStatusCode expectedStatus, AftTransferCode currentTransferCode)
        {
            const long expectedCashable = 100L;
            const long expectedRestricted = 321L;
            const long expectedNonRestricted = 123L;

            var data = new AftData
            {
                CashableAmount = (ulong)expectedCashable.CentsToMillicents(),
                NonRestrictedAmount = (ulong)expectedNonRestricted.CentsToMillicents(),
                RestrictedAmount = (ulong)expectedRestricted.CentsToMillicents(),
                TransferType = AftTransferType.GameToHostInHouse
            };

            AddMeterMock(expectedCashable, AccountingMeters.WatOffCashableAmount);
            AddMeterMock(expectedRestricted, AccountingMeters.WatOffNonCashableAmount);
            AddMeterMock(expectedNonRestricted, AccountingMeters.WatOffCashablePromoAmount);
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.AftTransferComplete)))
                .Verifiable();

            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferCode = currentTransferCode,
                TransferStatus = AftTransferStatusCode.TransferPending,
                PoolId = 100,
                Expiration = 250
            };

            _target.UpdateFinalAftResponseData(data);

            Assert.AreEqual(expectedStatus, _target.CurrentTransfer.TransferStatus);
            Assert.AreEqual(expectedCashable + expectedNonRestricted + expectedRestricted, (long)_target.TransferAmount);
            Assert.AreEqual(expectedCashable, _target.CurrentTransfer.CumulativeCashableAmount);
            Assert.AreEqual(expectedNonRestricted, _target.CurrentTransfer.CumulativeNonRestrictedAmount);
            Assert.AreEqual(expectedRestricted, _target.CurrentTransfer.CumulativeRestrictedAmount);
            Assert.AreEqual((uint)0, _target.CurrentTransfer.Expiration);
            Assert.AreEqual((ushort)0, _target.CurrentTransfer.PoolId);
        }

        [DataRow(
            AftTransferStatusCode.FullTransferSuccessful,
            AftTransferCode.TransferRequestFullTransferOnly,
            100L,
            321L,
            123L,
            30,
            30,
            (ushort)100,
            (ushort)100,
            DisplayName = "Successful Full Transfer")]
        [DataRow(
            AftTransferStatusCode.PartialTransferSuccessful,
            AftTransferCode.TransferRequestPartialTransferAllowed,
            100L,
            321L,
            123L,
            30,
            30,
            (ushort)100,
            (ushort)100,
            DisplayName = "Successful Partial Transfer")]
        [DataRow(
            AftTransferStatusCode.FullTransferSuccessful,
            AftTransferCode.TransferRequestFullTransferOnly,
            100L,
            0L,
            123L,
            30,
            0,
            (ushort)100,
            (ushort)0,
            DisplayName = "Zero restricted returns 0 for date and pool id")]
        [DataTestMethod]
        public void UpdateFinalAftResponseDataTransferOnTest(
            AftTransferStatusCode expectedStatus,
            AftTransferCode currentTransferCode,
            long cashable,
            long restricted,
            long nonRestricted,
            int expirationDate,
            int expectedExpirationDate,
            ushort poolId,
            ushort expectedPoolId)
        {
            var data = new AftData
            {
                CashableAmount = (ulong)cashable.CentsToMillicents(),
                NonRestrictedAmount = (ulong)nonRestricted.CentsToMillicents(),
                RestrictedAmount = (ulong)restricted.CentsToMillicents(),
                TransferType = AftTransferType.HostToGameInHouse
            };

            AddMeterMock(cashable, AccountingMeters.WatOnCashableAmount);
            AddMeterMock(restricted, AccountingMeters.WatOnNonCashableAmount);
            AddMeterMock(nonRestricted, AccountingMeters.WatOnCashablePromoAmount);
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _ticketingCoordinator.Setup(x => x.TicketExpirationRestricted).Returns((ulong)expectedExpirationDate);
            _bank.Setup(x => x.QueryBalance(AccountType.NonCash)).Returns(restricted.CentsToMillicents());
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.AftTransferComplete)))
                .Verifiable();

            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferStatus = AftTransferStatusCode.TransferPending,
                TransferCode = currentTransferCode,
                Expiration = (uint)expirationDate,
                PoolId = poolId
            };

            _target.UpdateFinalAftResponseData(data);

            Assert.AreEqual(expectedStatus, _target.CurrentTransfer.TransferStatus);
            Assert.AreEqual(cashable + nonRestricted + restricted, (long)_target.TransferAmount);
            Assert.AreEqual(cashable, _target.CurrentTransfer.CumulativeCashableAmount);
            Assert.AreEqual(nonRestricted, _target.CurrentTransfer.CumulativeNonRestrictedAmount);
            Assert.AreEqual(restricted, _target.CurrentTransfer.CumulativeRestrictedAmount);
            Assert.AreEqual(expectedPoolId, _target.CurrentTransfer.PoolId);
            Assert.AreEqual(expectedExpirationDate, (int)_target.CurrentTransfer.Expiration);
        }

        [TestMethod]
        public void UpdateFinalAftResponseDataTransferZeroExpirationOnTest()
        {
            const long expectedRestricted = 123L;
            const int defaultExpiration = 100;
            const ushort poolId = 2;

            var data = new AftData
            {
                CashableAmount = 0,
                NonRestrictedAmount = 0,
                RestrictedAmount = (ulong)expectedRestricted.CentsToMillicents(),
                TransferType = AftTransferType.HostToGameInHouse
            };

            AddMeterMock(0, AccountingMeters.WatOnCashableAmount);
            AddMeterMock(expectedRestricted, AccountingMeters.WatOnNonCashableAmount);
            AddMeterMock(0, AccountingMeters.WatOnCashablePromoAmount);
            _ticketingCoordinator.Setup(x => x.TicketExpirationRestricted).Returns(defaultExpiration);
            _ticketingCoordinator.Setup(x => x.DefaultTicketExpirationRestricted).Returns(defaultExpiration);
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            _bank.Setup(x => x.QueryBalance(AccountType.NonCash)).Returns(expectedRestricted.CentsToMillicents());
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.AftTransferComplete)))
                .Verifiable();

            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferStatus = AftTransferStatusCode.TransferPending,
                Expiration = 0,
                PoolId = poolId
            };

            _target.UpdateFinalAftResponseData(data);

            Assert.AreEqual(expectedRestricted, (long)_target.TransferAmount);
            Assert.AreEqual(expectedRestricted, _target.CurrentTransfer.CumulativeRestrictedAmount);
            Assert.AreEqual(poolId, _target.CurrentTransfer.PoolId);
            Assert.AreEqual(defaultExpiration, (int)_target.CurrentTransfer.Expiration);
        }

        [TestMethod]
        public void UpdateFinalAftResponseDataTransferBonusTest()
        {
            const long millicents = 1000;
            var expectedCashable = 100UL;
            var expectedNonRestricted = 123UL;
            var data = new AftData
            {
                CashableAmount = expectedCashable * millicents,
                NonRestrictedAmount = expectedNonRestricted * millicents,
                TransferType = AftTransferType.HostToGameBonusCoinOut
            };

            AddMeterMock((long)expectedCashable, SasMeterNames.AftCashableBonusIn);
            AddMeterMock((long)expectedNonRestricted, SasMeterNames.AftNonRestrictedBonusIn);
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.AftTransferComplete)))
                .Verifiable();

            _target.CurrentTransfer = new AftResponseData { TransferType = AftTransferType.HostToGameBonusCoinOut };

            _target.UpdateFinalAftResponseData(data);

            Assert.AreEqual(expectedCashable + expectedNonRestricted, _target.TransferAmount);
            Assert.AreEqual((long)expectedCashable, _target.CurrentTransfer.CumulativeCashableAmount);
            Assert.AreEqual((long)expectedNonRestricted, _target.CurrentTransfer.CumulativeNonRestrictedAmount);
            Assert.AreEqual(0, _target.CurrentTransfer.CumulativeRestrictedAmount);
        }

        [TestMethod]
        public void UpdateFinalAftResponseDataTransferBonusFailedTest()
        {
            var zero = 0;
            var cashable = 100UL;
            var expectedCashable = 0UL;
            var transferStatus = AftTransferStatusCode.GamingMachineUnableToPerformTransfer;
            var transactionDateTime = DateTime.MaxValue;

            var data = new AftData
            {
                CashableAmount = cashable
            };

            AddMeterMock(zero, SasMeterNames.AftCashableBonusIn);
            AddMeterMock(zero, SasMeterNames.AftNonRestrictedBonusIn);
            AddMeterMock(zero, AccountingMeters.WatOnCashableAmount);
            AddMeterMock(zero, AccountingMeters.WatOnNonCashableAmount);
            AddMeterMock(zero, AccountingMeters.WatOnCashablePromoAmount);

            _time.Setup(x => x.GetLocationTime(It.IsAny<DateTime>())).Returns(transactionDateTime);
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.AftTransferComplete)))
                .Verifiable();

            _target.UpdateFinalAftResponseData(data, transferStatus, true);

            Assert.AreEqual(expectedCashable, _target.CurrentTransfer.CashableAmount);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, _target.CurrentTransfer.ReceiptStatus);
            Assert.AreEqual(transactionDateTime, _target.CurrentTransfer.TransactionDateTime);
        }

        [TestMethod]
        public void AftTransferFailedTest()
        {
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);
            _target.CurrentTransfer = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                ReceiptStatus = (byte)ReceiptStatus.ReceiptPending,
                CashableAmount = 12345UL,
                NonRestrictedAmount = 23456UL,
                RestrictedAmount = 34567UL,
            };

            // check that date time is getting changed in the method
            Assert.AreEqual(DateTime.MinValue, _target.CurrentTransfer.TransactionDateTime);

            _target.AftTransferFailed();

            Assert.AreEqual(0UL, _target.CurrentTransfer.CashableAmount);
            Assert.AreEqual(0UL, _target.CurrentTransfer.NonRestrictedAmount);
            Assert.AreEqual(0UL, _target.CurrentTransfer.RestrictedAmount);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, _target.CurrentTransfer.ReceiptStatus);
            Assert.AreEqual(AftTransferType.HostToGameInHouse, _target.CurrentTransfer.TransferType);
            Assert.AreEqual(DateTime.MaxValue, _target.CurrentTransfer.TransactionDateTime);
        }

        private void AddMeterMock(long amount, string meterName)
        {
            const long millicents = 1000;
            var meterClassification = new CurrencyMeter();
            var meter = new Mock<IMeter>();
            meter.Setup(m => m.Lifetime).Returns(amount * millicents);
            meter.Setup(m => m.Classification).Returns(meterClassification);
            _meterManager.Setup(m => m.GetMeter(meterName)).Returns(meter.Object);
            _meterManager.Setup(m => m.IsMeterProvided(meterName)).Returns(true);
        }
    }

    internal class CurrencyMeter : MeterClassification
    {
        public CurrencyMeter()
            : base("Currency", 1000000L)
        {
        }

        public override string CreateValueString(long meterValue, CultureInfo culture = null)
        {
            return string.Empty;
        }
    }
}
