namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Sas.VoucherValidation;
    using Test.Common;
    using IPrinter = Hardware.Contracts.Printer.IPrinter;

    [TestClass]
    public class LP7BExtendedValidationStatusHandlerTests
    {
        private const int Timeout = 3000; // three seconds

        private LP7BExtendedValidationStatusHandler _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<ITicketingCoordinator> _ticketingCoordinator;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<ITicketDataProvider> _ticketDataProvider;
        private SasValidationHandlerFactory _sasValidationHandlerFactory;
        private Mock<IValidationHandler> _validationHandler;
        private Mock<INoteAcceptor> _noteAcceptor;
        private Mock<IPrinter> _printer;
        private Mock<IScopedTransaction> _scopedTransaction;
        private Mock<IStorageDataProvider<ValidationInformation>> _validationDataProvider;
        private ManualResetEvent _waitEvent;

        private static readonly ValidationControlStatus ValidFlags = Enum.GetValues(typeof(ValidationControlStatus))
            .Cast<ValidationControlStatus>().Aggregate(ValidationControlStatus.None, (current, value) => current | value);

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Default);
            _noteAcceptor = MoqServiceManager.CreateAndAddService<INoteAcceptor>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _persistentStorageManager = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _ticketDataProvider = new Mock<ITicketDataProvider>(MockBehavior.Default);
            _scopedTransaction = new Mock<IScopedTransaction>(MockBehavior.Default);
            _validationDataProvider = new Mock<IStorageDataProvider<ValidationInformation>>(MockBehavior.Default);

            _validationHandler = new Mock<IValidationHandler>(MockBehavior.Default);
            _validationHandler.SetupGet(x => x.ValidationType).Returns(SasValidationType.SecureEnhanced);
            _sasValidationHandlerFactory = new SasValidationHandlerFactory(
                _propertiesManager.Object,
                new List<IValidationHandler> { _validationHandler.Object });

            _waitEvent = new ManualResetEvent(false);
            _target = new LP7BExtendedValidationStatusHandler(
                _propertiesManager.Object,
                _validationDataProvider.Object,
                _persistentStorageManager.Object,
                _ticketingCoordinator.Object,
                _ticketDataProvider.Object,
                _transactionHistory.Object,
                _sasValidationHandlerFactory);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.ExtendedValidationStatus));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new LP7BExtendedValidationStatusHandler(
                null,
                _validationDataProvider.Object,
                _persistentStorageManager.Object,
                _ticketingCoordinator.Object,
                _ticketDataProvider.Object,
                _transactionHistory.Object,
                _sasValidationHandlerFactory);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPersistentStorageManagerTest()
        {
            _target = new LP7BExtendedValidationStatusHandler(
                _propertiesManager.Object,
                _validationDataProvider.Object,
                null,
                _ticketingCoordinator.Object,
                _ticketDataProvider.Object,
                _transactionHistory.Object,
                _sasValidationHandlerFactory);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTicketingCoordinatorTest()
        {
            _target = new LP7BExtendedValidationStatusHandler(
                _propertiesManager.Object,
                _validationDataProvider.Object,
                _persistentStorageManager.Object,
                null,
                _ticketDataProvider.Object,
                _transactionHistory.Object,
                _sasValidationHandlerFactory);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTicketDataProviderTest()
        {
            _target = new LP7BExtendedValidationStatusHandler(
                _propertiesManager.Object,
                _validationDataProvider.Object,
                _persistentStorageManager.Object,
                _ticketingCoordinator.Object,
                null,
                _transactionHistory.Object,
                _sasValidationHandlerFactory);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTransactionHistoryTest()
        {
            _target = new LP7BExtendedValidationStatusHandler(
                _propertiesManager.Object,
                _validationDataProvider.Object,
                _persistentStorageManager.Object,
                _ticketingCoordinator.Object,
                _ticketDataProvider.Object,
                null,
                _sasValidationHandlerFactory);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSasValidationHandlerFactoryTest()
        {
            _target = new LP7BExtendedValidationStatusHandler(
                _propertiesManager.Object,
                _validationDataProvider.Object,
                _persistentStorageManager.Object,
                _ticketingCoordinator.Object,
                _ticketDataProvider.Object,
                _transactionHistory.Object,
                null);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        public void HandleNoValidationTest()
        {
            const uint assetNumber = 51;
            const ulong cashableExpirationDays = 81;
            const ulong restrictedExpirationDays = 30;

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.None
            });

            _ticketingCoordinator.SetupGet(x => x.TicketExpirationCashable).Returns(cashableExpirationDays);
            _ticketingCoordinator.SetupGet(x => x.DefaultTicketExpirationRestricted).Returns(restrictedExpirationDays);

            var response = _target.Handle(
                new ExtendedValidationStatusData
                {
                    CashableExpirationDate = 90,
                    RestrictedExpirationDate = 91,
                    ControlStatus = ValidationControlStatus.UsePrinterAsHandPayDevice,
                    ControlMask = ValidationControlStatus.UsePrinterAsHandPayDevice
                });

            Assert.AreEqual(ValidationControlStatus.None, response.ControlStatus);
            Assert.AreEqual(assetNumber, response.AssertNumber);
            Assert.AreEqual((int)cashableExpirationDays, response.CashableExpirationDate);
            Assert.AreEqual((int)restrictedExpirationDays, response.RestrictedExpirationDate);
        }

        [TestMethod]
        public void CancelValidationTest()
        {
            const uint assetNumber = 51;
            const ulong cashableExpirationDays = 81;
            const ulong restrictedExpirationDays = 30;

            var expectedTicketData = new TicketData
            {
                DebitTicketTitle = "Test Debit Title",
                Address1 = "Address 1",
                Address2 = "Address 2",
                Location = "Location",
                RestrictedTicketTitle = "Restricted Ticket Title"
            };

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.SecureEnhanced
            });
            _persistentStorageManager.Setup(x => x.ScopedTransaction()).Returns(_scopedTransaction.Object);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultAddressLine1Key, It.IsAny<string>()))
                .Returns(expectedTicketData.Address1);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultAddressLine2Key, It.IsAny<string>()))
                .Returns(expectedTicketData.Address2);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultDebitTitleKey, It.IsAny<string>()))
                .Returns(expectedTicketData.DebitTicketTitle);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultLocationKey, It.IsAny<string>()))
                .Returns(expectedTicketData.Location);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.DefaultRestrictedTitleKey, It.IsAny<string>()))
                .Returns(expectedTicketData.RestrictedTicketTitle);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);
            ConfigureInitialStatus(ValidFlags);
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation());
            _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.ExtendedTicketDataStatus == TicketDataStatus.ValidData)))
                .Returns(Task.CompletedTask);
            _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.ExtendedTicketDataStatus == TicketDataStatus.InvalidData)))
                .Returns(Task.CompletedTask);
            _ticketingCoordinator.SetupGet(x => x.TicketExpirationCashable).Returns(cashableExpirationDays);
            _ticketingCoordinator.SetupGet(x => x.DefaultTicketExpirationRestricted).Returns(restrictedExpirationDays);
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData
            {
                TicketExpiration = (int)cashableExpirationDays
            });
            _validationHandler.Setup(x => x.Initialize()).Callback(() => _waitEvent.Set());
            _ticketDataProvider.Setup(
                    x => x.SetTicketData(
                        It.Is<TicketData>(
                            ticketData => ticketData.DebitTicketTitle == expectedTicketData.DebitTicketTitle &&
                                          ticketData.Address1 == expectedTicketData.Address1 &&
                                          ticketData.Address2 == expectedTicketData.Address2 &&
                                          ticketData.RestrictedTicketTitle ==
                                          expectedTicketData.RestrictedTicketTitle)))
                .Verifiable();

            var response = _target.Handle(
                new ExtendedValidationStatusData
                {
                    CashableExpirationDate = 90,
                    RestrictedExpirationDate = 91,
                    ControlStatus = ValidationControlStatus.UsePrinterAsHandPayDevice,
                    ControlMask = ValidationControlStatus.UsePrinterAsHandPayDevice | ValidationControlStatus.SecureEnhancedConfiguration
                });

            Assert.IsTrue(_waitEvent.WaitOne(Timeout));
            Assert.AreEqual(ValidationControlStatus.None, response.ControlStatus);
            Assert.AreEqual(assetNumber, response.AssertNumber);
        }

        [TestMethod]
        public void PrintDisabledRemovesPrintingStatues()
        {
            const ulong cashableExpirationDays = 81;
            const ulong restrictedExpirationDays = 30;
            const uint assetNumber = 51;
            var expectedStatus = ValidFlags & ~(ValidationControlStatus.PrintRestrictedTickets |
                                                ValidationControlStatus.PrintForeignRestrictedTickets |
                                                ValidationControlStatus.UsePrinterAsHandPayDevice |
                                                ValidationControlStatus.UsePrinterAsCashoutDevice);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);
            ConfigureInitialStatus(ValidFlags);
            ConfigureCurrentStatus(ValidFlags);
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                ValidationConfigured = true
            });
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.SecureEnhanced
            });
            _validationDataProvider.Setup(x => x.Save(It.IsAny<ValidationInformation>()))
                .Returns(Task.CompletedTask);
            _ticketingCoordinator.SetupGet(x => x.TicketExpirationCashable).Returns(cashableExpirationDays);
            _ticketingCoordinator.SetupGet(x => x.DefaultTicketExpirationRestricted).Returns(restrictedExpirationDays);
            _printer.Setup(x => x.CanPrint).Returns(false);
            _noteAcceptor.Setup(x => x.Enabled).Returns(true);
            _transactionHistory.Setup(x => x.RecallTransactions(It.IsAny<bool>()))
                .Returns(new List<ITransaction>().OrderBy(x => x.TransactionId));

            var response = _target.Handle(
                new ExtendedValidationStatusData
                {
                    CashableExpirationDate = (int)cashableExpirationDays,
                    RestrictedExpirationDate = (int)restrictedExpirationDays,
                    ControlStatus = ValidationControlStatus.None,
                    ControlMask = ValidationControlStatus.None
                });

            Assert.AreEqual(expectedStatus, response.ControlStatus);
            Assert.AreEqual(assetNumber, response.AssertNumber);
            Assert.AreEqual((int)cashableExpirationDays, response.CashableExpirationDate);
            Assert.AreEqual((int)restrictedExpirationDays, response.RestrictedExpirationDate);
        }

        [TestMethod]
        public void NoteAcceptorDisabledRemovesRedemptionStatues()
        {
            const ulong cashableExpirationDays = 81;
            const ulong restrictedExpirationDays = 30;
            const uint assetNumber = 51;
            var expectedStatus = ValidFlags & ~(ValidationControlStatus.TicketRedemption);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);
            ConfigureInitialStatus(ValidFlags);
            ConfigureCurrentStatus(ValidFlags);
            _ticketingCoordinator.SetupGet(x => x.TicketExpirationCashable).Returns(cashableExpirationDays);
            _ticketingCoordinator.SetupGet(x => x.DefaultTicketExpirationRestricted).Returns(restrictedExpirationDays);
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                ValidationConfigured = true
            });
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.SecureEnhanced
            });
            _validationDataProvider.Setup(x => x.Save(It.IsAny<ValidationInformation>()))
                .Returns(Task.CompletedTask);
            _printer.Setup(x => x.CanPrint).Returns(true);
            _noteAcceptor.Setup(x => x.Enabled).Returns(false);
            _transactionHistory.Setup(x => x.RecallTransactions(It.IsAny<bool>()))
                .Returns(new List<ITransaction>().OrderBy(x => x.TransactionId));

            var response = _target.Handle(
                new ExtendedValidationStatusData
                {
                    CashableExpirationDate = (int)cashableExpirationDays,
                    RestrictedExpirationDate = (int)restrictedExpirationDays,
                    ControlStatus = ValidationControlStatus.None,
                    ControlMask = ValidationControlStatus.None
                });

            Assert.AreEqual(expectedStatus, response.ControlStatus);
            Assert.AreEqual(assetNumber, response.AssertNumber);
            Assert.AreEqual((int)cashableExpirationDays, response.CashableExpirationDate);
            Assert.AreEqual((int)restrictedExpirationDays, response.RestrictedExpirationDate);
        }

        [TestMethod]
        public void SystemValidationRemovesSecureEnhancedStatues()
        {
            const ulong cashableExpirationDays = 81;
            const ulong restrictedExpirationDays = 30;
            const uint assetNumber = 51;
            var expectedStatus = ValidFlags & ~(ValidationControlStatus.ValidateHandPays |
                                                ValidationControlStatus.SecureEnhancedConfiguration |
                                                ValidationControlStatus.UsePrinterAsHandPayDevice);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);
            ConfigureInitialStatus(ValidFlags);
            ConfigureCurrentStatus(ValidFlags);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.System
            });
            _validationDataProvider.Setup(x => x.Save(It.IsAny<ValidationInformation>()))
                .Returns(Task.CompletedTask);
            _ticketingCoordinator.SetupGet(x => x.TicketExpirationCashable).Returns(cashableExpirationDays);
            _ticketingCoordinator.SetupGet(x => x.DefaultTicketExpirationRestricted).Returns(restrictedExpirationDays);
            _printer.Setup(x => x.CanPrint).Returns(true);
            _noteAcceptor.Setup(x => x.Enabled).Returns(true);
            _transactionHistory.Setup(x => x.RecallTransactions(It.IsAny<bool>()))
                .Returns(new List<ITransaction>().OrderBy(x => x.TransactionId));

            var response = _target.Handle(
                new ExtendedValidationStatusData
                {
                    CashableExpirationDate = (int)cashableExpirationDays,
                    RestrictedExpirationDate = (int)restrictedExpirationDays,
                    ControlStatus = ValidationControlStatus.None,
                    ControlMask = ValidationControlStatus.None
                });

            Assert.AreEqual(expectedStatus, response.ControlStatus);
            Assert.AreEqual(assetNumber, response.AssertNumber);
            Assert.AreEqual((int)cashableExpirationDays, response.CashableExpirationDate);
            Assert.AreEqual((int)restrictedExpirationDays, response.RestrictedExpirationDate);
        }

        [TestMethod]
        public void ValidatonTypeNoneAlwaysReturnsNoStatus()
        {
            const ulong cashableExpirationDays = 81;
            const ulong restrictedExpirationDays = 30;
            const uint assetNumber = 51;

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.None
            });
            _ticketingCoordinator.SetupGet(x => x.TicketExpirationCashable).Returns(cashableExpirationDays);
            _ticketingCoordinator.SetupGet(x => x.DefaultTicketExpirationRestricted).Returns(restrictedExpirationDays);

            var response = _target.Handle(
                new ExtendedValidationStatusData
                {
                    CashableExpirationDate = (int)cashableExpirationDays,
                    RestrictedExpirationDate = (int)restrictedExpirationDays,
                    ControlStatus = ValidationControlStatus.None,
                    ControlMask = ValidationControlStatus.None
                });

            Assert.AreEqual(ValidationControlStatus.None, response.ControlStatus);
            Assert.AreEqual(assetNumber, response.AssertNumber);
            Assert.AreEqual((int)cashableExpirationDays, response.CashableExpirationDate);
            Assert.AreEqual((int)restrictedExpirationDays, response.RestrictedExpirationDate);
        }

        [DataRow(
            true,
            true,
            ValidationControlStatus.ValidateHandPays | ValidationControlStatus.SecureEnhancedConfiguration,
            ValidationControlStatus.ValidateHandPays | ValidationControlStatus.SecureEnhancedConfiguration,
            ValidationControlStatus.ValidateHandPays,
            ValidationControlStatus.None,
            // TODO : Remove ValidateHandPays when handing the TODO in the LP7B handler.
            ValidationControlStatus.ValidateHandPays | ValidationControlStatus.SecureEnhancedConfiguration,
            DisplayName = "Clearing Validating handpays removes is ignored")]
        [DataRow(
            true,
            false,
            ValidationControlStatus.ValidateHandPays | ValidationControlStatus.SecureEnhancedConfiguration,
            ValidationControlStatus.ValidateHandPays | ValidationControlStatus.SecureEnhancedConfiguration,
            ValidationControlStatus.ValidateHandPays,
            ValidationControlStatus.ValidateHandPays,
            ValidationControlStatus.None,
            DisplayName = "When validation is uninitialized then we return not current validation status")]
        [DataRow(
            true,
            true,
            ValidationControlStatus.PrintRestrictedTickets | ValidationControlStatus.UsePrinterAsCashoutDevice | ValidationControlStatus.SecureEnhancedConfiguration,
            ValidationControlStatus.SecureEnhancedConfiguration,
            ValidationControlStatus.PrintRestrictedTickets | ValidationControlStatus.UsePrinterAsCashoutDevice,
            ValidationControlStatus.PrintRestrictedTickets | ValidationControlStatus.UsePrinterAsCashoutDevice,
            ValidationControlStatus.PrintRestrictedTickets | ValidationControlStatus.PrintForeignRestrictedTickets | ValidationControlStatus.UsePrinterAsCashoutDevice | ValidationControlStatus.SecureEnhancedConfiguration,
            DisplayName = "Configuring print cashable and restricted tickets")]
        [DataRow(
            false,
            true,
            ValidationControlStatus.TicketRedemption | ValidationControlStatus.SecureEnhancedConfiguration,
            ValidationControlStatus.TicketRedemption | ValidationControlStatus.SecureEnhancedConfiguration,
            ValidationControlStatus.TicketRedemption,
            ValidationControlStatus.TicketRedemption,
            (ValidationControlStatus.TicketRedemption | ValidationControlStatus.SecureEnhancedConfiguration),
            DisplayName = "Configuring ticket redemption")]
        [DataTestMethod]
        public void ConfigureValidationStatuses(
            bool persit,
            bool validationConfigured,
            ValidationControlStatus initialStatus,
            ValidationControlStatus currentStatus,
            ValidationControlStatus maskStatus,
            ValidationControlStatus controlStatus,
            ValidationControlStatus expectedStatus)
        {
            const ulong cashableExpirationDays = 81;
            const ulong restrictedExpirationDays = 30;
            const uint assetNumber = 51;

            _persistentStorageManager.Setup(x => x.ScopedTransaction()).Returns(_scopedTransaction.Object);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.MachineId, (uint)0)).Returns(assetNumber);
            ConfigureInitialStatus(initialStatus);
            ConfigureCurrentStatus(currentStatus);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures
            {
                ValidationType = SasValidationType.SecureEnhanced
            });
            _validationDataProvider.Setup(x => x.GetData()).Returns(new ValidationInformation
            {
                ValidationConfigured = validationConfigured
            });
            _validationDataProvider.Setup(x => x.Save(It.Is<ValidationInformation>(v => v.ExtendedTicketDataStatus == TicketDataStatus.ValidData)))
                .Returns(Task.CompletedTask);
            _ticketingCoordinator.SetupGet(x => x.TicketExpirationCashable).Returns(cashableExpirationDays);
            _ticketingCoordinator.SetupGet(x => x.DefaultTicketExpirationRestricted).Returns(restrictedExpirationDays);
            _printer.Setup(x => x.CanPrint).Returns(true);
            _noteAcceptor.Setup(x => x.Enabled).Returns(true);
            _transactionHistory.Setup(x => x.RecallTransactions(It.IsAny<bool>()))
                .Returns(new List<ITransaction>().OrderBy(x => x.TransactionId));
            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData());
            if (persit)
            {
                SetupPersistedValues(expectedStatus);
                _ticketingCoordinator.Setup(x => x.Save(It.IsAny<TicketStorageData>())).Returns(Task.CompletedTask);
                _scopedTransaction.Setup(x => x.Complete()).Callback(() => _waitEvent.Set());
            }

            var response = _target.Handle(
                new ExtendedValidationStatusData
                {
                    CashableExpirationDate = (int)cashableExpirationDays,
                    RestrictedExpirationDate = (int)restrictedExpirationDays,
                    ControlStatus = controlStatus,
                    ControlMask = maskStatus
                });

            if (persit)
            {
                Assert.IsTrue(_waitEvent.WaitOne(Timeout));
            }

            Assert.AreEqual(expectedStatus, response.ControlStatus);
            Assert.AreEqual(assetNumber, response.AssertNumber);
            Assert.AreEqual((int)cashableExpirationDays, response.CashableExpirationDate);
            Assert.AreEqual((int)restrictedExpirationDays, response.RestrictedExpirationDate);
        }

        private void SetupPersistedValues(ValidationControlStatus status)
        {
            _propertiesManager.Setup(
                x => x.SetProperty(
                    AccountingConstants.VoucherOut,
                    (status & ValidationControlStatus.UsePrinterAsCashoutDevice) != 0)).Verifiable();
            _propertiesManager.Setup(
                x => x.SetProperty(
                    AccountingConstants.ValidateHandpays,
                    (status & ValidationControlStatus.ValidateHandPays) != 0)).Verifiable();
            _propertiesManager.Setup(
                x => x.SetProperty(
                    AccountingConstants.EnableReceipts,
                    (status & ValidationControlStatus.UsePrinterAsHandPayDevice) != 0)).Verifiable();
            _propertiesManager.Setup(
                x => x.SetProperty(
                    AccountingConstants.VoucherOutNonCash,
                    (status & ValidationControlStatus.PrintRestrictedTickets) != 0)).Verifiable();
            _propertiesManager.Setup(
                x => x.SetProperty(
                    PropertyKey.VoucherIn,
                    (status & ValidationControlStatus.TicketRedemption) != 0)).Verifiable();
        }

        private void ConfigureInitialStatus(ValidationControlStatus status)
        {
            _propertiesManager.Setup(
                    x => x.GetProperty(SasProperties.PrinterAsCashOutDeviceSupportedKey, It.IsAny<bool>()))
                .Returns((status & ValidationControlStatus.UsePrinterAsCashoutDevice) != 0);
            _propertiesManager.Setup(
                    x => x.GetProperty(SasProperties.HandPayValidationSupportedKey, It.IsAny<bool>()))
                .Returns((status & ValidationControlStatus.ValidateHandPays) != 0);
            _propertiesManager.Setup(
                    x => x.GetProperty(SasProperties.PrinterAsHandPayDeviceSupportedKey, It.IsAny<bool>()))
                .Returns((status & ValidationControlStatus.UsePrinterAsHandPayDevice) != 0);
            _propertiesManager.Setup(
                    x => x.GetProperty(SasProperties.ForeignRestrictedTicketsSupportedKey, It.IsAny<bool>()))
                .Returns((status & ValidationControlStatus.PrintForeignRestrictedTickets) != 0);
            _propertiesManager.Setup(
                    x => x.GetProperty(SasProperties.RestrictedTicketsSupportedKey, It.IsAny<bool>()))
                .Returns((status & ValidationControlStatus.PrintRestrictedTickets) != 0);
            _propertiesManager.Setup(
                    x => x.GetProperty(SasProperties.TicketRedemptionSupportedKey, It.IsAny<bool>()))
                .Returns((status & ValidationControlStatus.TicketRedemption) != 0);
        }

        private void ConfigureCurrentStatus(ValidationControlStatus status)
        {
            _propertiesManager.Setup(x => x.GetProperty(PropertyKey.VoucherIn, It.IsAny<bool>()))
                .Returns(status.HasFlag(ValidationControlStatus.TicketRedemption));
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOut, It.IsAny<bool>()))
                .Returns(status.HasFlag(ValidationControlStatus.UsePrinterAsCashoutDevice));
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.VoucherOutNonCash, It.IsAny<bool>()))
                .Returns(status.HasFlag(ValidationControlStatus.PrintRestrictedTickets));
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.ValidateHandpays, It.IsAny<bool>()))
                .Returns(status.HasFlag(ValidationControlStatus.ValidateHandPays));
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.EnableReceipts, It.IsAny<bool>()))
                .Returns(status.HasFlag(ValidationControlStatus.UsePrinterAsHandPayDevice));
        }
    }
}