namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;
    using Sas.AftTransferProvider;
    using Test.Common;
    using IPrinter = Hardware.Contracts.Printer.IPrinter;

    /// <summary>
    ///     Contains the unit tests for the AftTransferFullPartial class
    /// </summary>
    [TestClass]
    public class AftTransferFullPartialTest
    {
        private AftTransferFullPartial _target;
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Default);
        private Mock<IAftRequestProcessor> _aftTransferBonusCoinOutWinFromHostToGamingMachine;
        private Mock<IAftRequestProcessor> _aftTransferBonusJackpotWinFromHostToGamingMachine;
        private Mock<IAftRequestProcessor> _aftTransferDebitFromHostToGamingMachine;
        private Mock<IAftRequestProcessor> _aftTransferDebitFromHostToTicket;
        private Mock<IAftRequestProcessor> _aftTransferInHouseFromGameMachineToHost;
        private Mock<IAftRequestProcessor> _aftTransferInHouseFromHostToGameMachine;
        private Mock<IAftRequestProcessor> _aftTransferInHouseFromHostToTicket;
        private Mock<IAftRequestProcessor> _aftTransferWinAmountFromGameMachineToHost;
        private readonly Mock<ITicketingCoordinator> _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IAftRegistrationProvider> _registrationProvider = new Mock<IAftRegistrationProvider>(MockBehavior.Default);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Strict);
        private readonly Mock<IAftLockHandler> _aftLockHandler = new Mock<IAftLockHandler>(MockBehavior.Default);
        private readonly Mock<ISasBonusCallback> _bonus = new Mock<ISasBonusCallback>(MockBehavior.Default);
        private readonly Mock<IAftHostCashOutProvider> _cashout = new Mock<IAftHostCashOutProvider>(MockBehavior.Default);
        private readonly Mock<IAftHostCashOutProvider> _hostCashoutProvider = new Mock<IAftHostCashOutProvider>(MockBehavior.Default);
        private readonly Mock<ITime> _time = new Mock<ITime>(MockBehavior.Strict);
        private readonly Mock<IFundsTransferDisable> _fundsTransferDisable = new Mock<IFundsTransferDisable>(MockBehavior.Default);
        private readonly Mock<IAutoPlayStatusProvider> _autoPlayStatusProvider = new Mock<IAutoPlayStatusProvider>(MockBehavior.Default);
        private readonly Mock<IAftRegistrationProvider> _registerProvider = new Mock<IAftRegistrationProvider>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank.Setup(m => m.Limit).Returns(0);
            _aftProvider.Setup(x => x.TransactionIdUnique).Returns(true);
            _aftProvider.Setup(x => x.TransactionIdValid).Returns(true);
            _aftProvider.Setup(x => x.TransferLimitAmount).Returns(1_000ul);
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(true);
            _aftProvider.Setup(x => x.PartialTransfersAllowed).Returns(true);
            _aftProvider.Setup(x => x.IsRegistrationKeyAllZeros).Returns(true);

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { PartialTransferAllowed = true, TransferInAllowed = true });
            _hostCashoutProvider.Setup(x => x.HostCashOutPending).Returns(false);
            _aftTransferBonusCoinOutWinFromHostToGamingMachine =
                new Mock<AftTransferBonusCoinOutWinFromHostToGamingMachine>(_aftProvider.Object, _bonus.Object, _propertiesManager.Object).As<IAftRequestProcessor>();
            _aftTransferBonusJackpotWinFromHostToGamingMachine =
                new Mock<AftTransferBonusJackpotWinFromHostToGamingMachine>(_aftProvider.Object, _bonus.Object, _propertiesManager.Object).As<IAftRequestProcessor>();
            _aftTransferDebitFromHostToGamingMachine =
                new Mock<AftTransferDebitFromHostToGamingMachine>(_aftProvider.Object, _registrationProvider.Object).As<IAftRequestProcessor>();
            _aftTransferDebitFromHostToTicket =
                new Mock<AftTransferDebitFromHostToTicket>(_aftProvider.Object, _registrationProvider.Object).As<IAftRequestProcessor>();
            _aftTransferInHouseFromGameMachineToHost =
                new Mock<AftTransferInHouseFromGameMachineToHost>(_aftProvider.Object, _cashout.Object, _propertiesManager.Object).As<IAftRequestProcessor>();
            _aftTransferInHouseFromHostToGameMachine =
                new Mock<AftTransferInHouseFromHostToGameMachine>(_aftProvider.Object, _ticketingCoordinator.Object, _bank.Object, _propertiesManager.Object).As<IAftRequestProcessor>();
            _aftTransferInHouseFromHostToTicket =
                new Mock<AftTransferInHouseFromHostToTicket>(_aftProvider.Object).As<IAftRequestProcessor>();
            _aftTransferWinAmountFromGameMachineToHost =
                new Mock<AftTransferWinAmountFromGameMachineToHost>(_aftProvider.Object, _cashout.Object, _propertiesManager.Object).As<IAftRequestProcessor>();

            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusCoinOutWinFromHostToGamingMachine.Object,
                _aftTransferBonusJackpotWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToTicket.Object,
                _aftTransferInHouseFromGameMachineToHost.Object,
                _aftTransferInHouseFromHostToGameMachine.Object,
                _aftTransferInHouseFromHostToTicket.Object,
                _aftTransferWinAmountFromGameMachineToHost.Object
            };

            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);

            _target = CreateTarget(aftRequestProcessors);
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTest(
            bool nullAftProvider,
            bool nullHostCashoutProvider,
            bool nullFundsTransferDisable,
            bool nullAutoPlayStatusProvider,
            bool nullRegistration)
        {
            _target = CreateTarget(
                Enumerable.Empty<IAftRequestProcessor>(),
                nullAftProvider,
                nullHostCashoutProvider,
                nullFundsTransferDisable,
                nullAutoPlayStatusProvider,
                nullRegistration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingBonusCoinOutTest()
        {
            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusJackpotWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToTicket.Object,
                _aftTransferInHouseFromGameMachineToHost.Object,
                _aftTransferInHouseFromHostToGameMachine.Object,
                _aftTransferInHouseFromHostToTicket.Object,
                _aftTransferWinAmountFromGameMachineToHost.Object
            };

            _target = CreateTarget(aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingBonusJackpotTest()
        {
            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusCoinOutWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToTicket.Object,
                _aftTransferInHouseFromGameMachineToHost.Object,
                _aftTransferInHouseFromHostToGameMachine.Object,
                _aftTransferInHouseFromHostToTicket.Object,
                _aftTransferWinAmountFromGameMachineToHost.Object
            };

            _target = CreateTarget(aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingDebitFromHostToGamingMachineTest()
        {
            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusCoinOutWinFromHostToGamingMachine.Object,
                _aftTransferBonusJackpotWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToTicket.Object,
                _aftTransferInHouseFromGameMachineToHost.Object,
                _aftTransferInHouseFromHostToGameMachine.Object,
                _aftTransferInHouseFromHostToTicket.Object,
                _aftTransferWinAmountFromGameMachineToHost.Object
            };

            _target = CreateTarget(aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingDebitFromHostToTicketTest()
        {
            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusCoinOutWinFromHostToGamingMachine.Object,
                _aftTransferBonusJackpotWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToGamingMachine.Object,
                _aftTransferInHouseFromGameMachineToHost.Object,
                _aftTransferInHouseFromHostToGameMachine.Object,
                _aftTransferInHouseFromHostToTicket.Object,
                _aftTransferWinAmountFromGameMachineToHost.Object
            };

            _target = CreateTarget(aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingGameMachineToHostTest()
        {
            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusCoinOutWinFromHostToGamingMachine.Object,
                _aftTransferBonusJackpotWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToTicket.Object,
                _aftTransferInHouseFromHostToGameMachine.Object,
                _aftTransferInHouseFromHostToTicket.Object,
                _aftTransferWinAmountFromGameMachineToHost.Object
            };

            _target = CreateTarget(aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingHostToGameMachineTest()
        {
            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusCoinOutWinFromHostToGamingMachine.Object,
                _aftTransferBonusJackpotWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToTicket.Object,
                _aftTransferInHouseFromGameMachineToHost.Object,
                _aftTransferInHouseFromHostToTicket.Object,
                _aftTransferWinAmountFromGameMachineToHost.Object
            };

            _target = CreateTarget(aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingHostToTicketTest()
        {
            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusCoinOutWinFromHostToGamingMachine.Object,
                _aftTransferBonusJackpotWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToTicket.Object,
                _aftTransferInHouseFromGameMachineToHost.Object,
                _aftTransferInHouseFromHostToGameMachine.Object,
                _aftTransferWinAmountFromGameMachineToHost.Object
            };

            _target = CreateTarget(aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingWinAmountFromGameMachineToHostTest()
        {
            var aftRequestProcessors = new List<IAftRequestProcessor>
            {
                _aftTransferBonusCoinOutWinFromHostToGamingMachine.Object,
                _aftTransferBonusJackpotWinFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToGamingMachine.Object,
                _aftTransferDebitFromHostToTicket.Object,
                _aftTransferInHouseFromGameMachineToHost.Object,
                _aftTransferInHouseFromHostToGameMachine.Object,
                _aftTransferInHouseFromHostToTicket.Object
            };

            _target = CreateTarget(aftRequestProcessors);
        }

        [TestMethod]
        public void HandleHandlerNotFoundTest()
        {
            // request invalid transfer type so handler won't be found
            var data = new AftResponseData
            {
                TransferType = (AftTransferType)0x03,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = 0,
                TransactionIndex = 0
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.UnsupportedTransferCode, response.TransferStatus);
        }

        [TestMethod]
        public void HandleDuplicateTransferTest()
        {
            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouseWin,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
                AssetNumber = 0,
                TransactionIndex = 0
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.Setup(x => x.IsTransferInProgress).Returns(true);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(false);
            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
        }

        [TestMethod]
        public void HostToGameBonusCoinOut()
        {
            HandleGoodTransferTest(AftTransferType.HostToGameBonusCoinOut, _aftTransferBonusCoinOutWinFromHostToGamingMachine);
        }

        [TestMethod]
        public void HostToGameBonusJackpot()
        {
            HandleGoodTransferTest(AftTransferType.HostToGameBonusJackpot, _aftTransferBonusJackpotWinFromHostToGamingMachine);
        }

        [TestMethod]
        public void HostToGameDebit()
        {
            HandleGoodTransferTest(AftTransferType.HostToGameDebit, _aftTransferDebitFromHostToGamingMachine);
        }

        [TestMethod]
        public void HostToGameDebitTicket()
        {
            HandleGoodTransferTest(AftTransferType.HostToGameDebitTicket, _aftTransferDebitFromHostToTicket);
        }

        [TestMethod]
        public void GameToHostInHouse()
        {
            HandleGoodTransferTest(AftTransferType.GameToHostInHouse, _aftTransferInHouseFromGameMachineToHost);
        }

        [TestMethod]
        public void HostToGameInHouse()
        {
            HandleGoodTransferTest(AftTransferType.HostToGameInHouse, _aftTransferInHouseFromHostToGameMachine);
        }

        [TestMethod]
        public void HostToGameInHouseTicket()
        {
            HandleGoodTransferTest(AftTransferType.HostToGameInHouseTicket, _aftTransferInHouseFromHostToTicket);
        }

        [TestMethod]
        public void GameToHostInHouseWin()
        {
            HandleGoodTransferTest(AftTransferType.GameToHostInHouseWin, _aftTransferWinAmountFromGameMachineToHost);
        }

        [TestMethod]
        public void GameToHostInHouseDuringHostCashOut()
        {
            _hostCashoutProvider.Setup(x => x.HostCashOutPending).Returns(true);
            _aftProvider.Setup(x => x.TransferOff).Returns(true);
            HandleGoodTransferTest(AftTransferType.GameToHostInHouse, _aftTransferInHouseFromGameMachineToHost);
        }

        [TestMethod]
        public void GameToHostInHouseWinDuringHostCashOut()
        {
            _hostCashoutProvider.Setup(x => x.HostCashOutPending).Returns(true);
            _aftProvider.Setup(x => x.TransferOff).Returns(true);
            HandleGoodTransferTest(AftTransferType.GameToHostInHouseWin, _aftTransferWinAmountFromGameMachineToHost);
        }

        public void HandleGoodTransferTest(AftTransferType transferType, Mock<IAftRequestProcessor> processor)
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledInGame).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledTilt).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledOverlay).Returns(false);
            _bank.Setup(m => m.Limit).Returns(100L);

            var data = new AftResponseData
            {
                TransferType = transferType,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
                ReceiptData = new AftReceiptData { PatronAccount = "1234", DebitCardNumber = "4567" }
            };

            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.NotCompatibleWithCurrentTransfer,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);
            _aftProvider.SetupErrorHandler(data);
            processor.Setup(m => m.Process(It.IsAny<AftResponseData>())).Returns(expected);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.NotCompatibleWithCurrentTransfer, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleBadTransactionIdTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouseWin,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = string.Empty,
                CashableAmount = 12345ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransactionIdValid).Returns(false);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.TransactionIdNotValid, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void HandleWrongTransactionIndexTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameBonusJackpot,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 1,
                TransactionId = "abc",
                CashableAmount = 12345ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void HandleAssetNumberZeroTest()
        {
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameDebit,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = 0,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.AssetNumberZeroOrDoesNotMatch, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void HandleAssetNumberMatchFailTest()
        {
            _aftProvider.Setup(x => x.AssetNumber).Returns((uint)456);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameDebitTicket,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = 1,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
                RegistrationKey = new byte[20]
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.AssetNumberZeroOrDoesNotMatch, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void HandleLockedTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
                TransferFlags = AftTransferFlags.AcceptTransferOnlyIfLocked
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);
            _aftProvider.Setup(x => x.IsLocked).Returns(false);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferFlagSet(AftTransferFlags.AcceptTransferOnlyIfLocked))
                .Returns(true);
            _aftLockHandler.Setup(m => m.LockStatus).Returns(AftGameLockStatus.GameNotLocked);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineNotLocked, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void HandleAutoPlayActiveTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(true);
                        var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineUnableToPerformTransfer, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void HandlePrintReceiptFailTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouseTicket,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
                TransferFlags = AftTransferFlags.TransactionReceiptRequested
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.Setup(x => x.IsTransferFlagSet(AftTransferFlags.TransactionReceiptRequested)).Returns(true);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsPrinterAvailable).Returns(false);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            printer.Setup(m => m.CanPrint).Returns(false);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.UnableToPrintTransactionReceipt, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void HandlePrintReceiptTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);
            _bank.Setup(m => m.Limit).Returns(100L);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouseTicket,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
                TransferFlags = AftTransferFlags.TransactionReceiptRequested,
                ReceiptData = new AftReceiptData
                {
                    PatronAccount = "1234"
                }
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            printer.Setup(m => m.CanPrint).Returns(true);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledInGame).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledTilt).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledOverlay).Returns(false);

            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.ReceiptPending
            };

            _aftTransferInHouseFromHostToTicket.Setup(m => m.Process(data)).Returns(expected);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPending, response.ReceiptStatus);
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void HandlePrintReceiptWithMissingFieldsTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouseTicket,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
                TransferFlags = AftTransferFlags.TransactionReceiptRequested,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.MissingRequiredReceiptFields).Returns(true);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            var printer = MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Strict);
            printer.Setup(m => m.CanPrint).Returns(true);

            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.ReceiptPending
            };

            _aftTransferInHouseFromHostToTicket.Setup(m => m.Process(data)).Returns(expected);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.InsufficientDataToPrintTransactionReceipt, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void HandlePartialNotConfiguredTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { PartialTransferAllowed = false, TransferInAllowed = true });
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(false);
            _aftProvider.Setup(x => x.PartialTransfersAllowed).Returns(false);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineUnableToPerformPartial, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [DataRow(true, false, AftTransferStatusCode.RegistrationKeyDoesNotMatch)]
        [DataRow(false, true, AftTransferStatusCode.GamingMachineNotRegistered)]
        [DataTestMethod]
        public void HandleRegistrationKeyNotZeroTest(bool registered, bool keyMatched, AftTransferStatusCode expectedStatus)
        {
            const uint assetNumber = 456;
            var registrationKey = new byte[] { 0x01, 0x00 };
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouseTicket,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
                RegistrationKey = registrationKey
            };

            _registerProvider.Setup(x => x.IsAftRegistered).Returns(registered);
            _aftProvider.Setup(x => x.IsRegistrationKeyAllZeros).Returns(false);
            _registerProvider.Setup(x => x.RegistrationKeyMatches(registrationKey)).Returns(keyMatched);
            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.ReceiptPending
            };

            _aftTransferInHouseFromHostToTicket.Setup(m => m.Process(data)).Returns(expected);

            var response = _target.Process(data);

            Assert.AreEqual(expectedStatus, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [DataRow(AftTransferType.HostToGameBonusCoinOut, DisplayName = "Host to Game Bonus Coin Out can't happen during host cashout")]
        [DataRow(AftTransferType.HostToGameBonusJackpot, DisplayName = "Host to Game Bonus Jackpot can't happen during host cashout")]
        [DataRow(AftTransferType.HostToGameDebit, DisplayName = "Host to Game Debit can't happen during host cashout")]
        [DataRow(AftTransferType.HostToGameDebitTicket, DisplayName = "Host to Game Debit Ticket can't happen during host cashout")]
        [DataRow(AftTransferType.HostToGameInHouse, DisplayName = "Host to Game In House can't happen during host cashout")]
        [DataRow(AftTransferType.HostToGameInHouseTicket, DisplayName = "Host to Game In House Ticket can't happen during host cashout")]
        [DataTestMethod]
        public void AftInHostCashOutFailures(AftTransferType transferType)
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _hostCashoutProvider.Setup(x => x.HostCashOutPending).Returns(true);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);

            var data = new AftResponseData
            {
                TransferType = transferType,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineUnableToPerformTransfer, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [DataRow(true, false, DisplayName = "Funds Transfer On SystemDisabled")]
        [DataRow(false, true, DisplayName = "Funds Transfer when OverlayWindow")]
        [DataTestMethod]
        public void HandleTransferOnTiltAndOverlayTest(bool isSystemDisabled, bool isOverlayActive)
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledTilt).Returns(isSystemDisabled);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledOverlay).Returns(isOverlayActive);
            _bank.Setup(m => m.Limit).Returns(100L);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
                ReceiptData = new AftReceiptData { PatronAccount = "1234", DebitCardNumber = "4567" }
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineUnableToPerformTransfer, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleTransferOnDisabledByGameTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledInGame).Returns(true);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledTilt).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledOverlay).Returns(false);
            _bank.Setup(m => m.Limit).Returns(100L);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineUnableToPerformTransfer, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleTransferOffDisabledTest()
        {
            uint assetNumber = 456;
            _aftProvider.Setup(x => x.AssetNumber).Returns(assetNumber);
            _autoPlayStatusProvider.Setup(m => m.EndAutoPlayIfActive()).Returns(false);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(true);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledInGame).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledTilt).Returns(false);
            _fundsTransferDisable.Setup(m => m.TransferOnDisabledOverlay).Returns(false);
            _hostCashoutProvider.Setup(m => m.CanCashOut).Returns(false);
            _bank.Setup(m => m.Limit).Returns(100L);

            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                AssetNumber = assetNumber,
                TransactionIndex = 0,
                TransactionId = "abc",
                CashableAmount = 12345ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.Setup(x => x.TransferOff).Returns(true);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.IsTransferAcknowledgedByHost).Returns(true);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineUnableToPerformTransfer, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        private AftTransferFullPartial CreateTarget(
            IEnumerable<IAftRequestProcessor> processors,
            bool nullAftProvider = false,
            bool nullHostCashoutProvider = false,
            bool nullFundsTransferDisable = false,
            bool nullAutoPlayStatusProvider = false,
            bool nullRegistration = false)
        {
            return new AftTransferFullPartial(
                nullAftProvider ? null : _aftProvider.Object,
                nullHostCashoutProvider ? null : _hostCashoutProvider.Object,
                nullFundsTransferDisable ? null : _fundsTransferDisable.Object,
                nullAutoPlayStatusProvider ? null : _autoPlayStatusProvider.Object,
                nullRegistration ? null : _registerProvider.Object,
                processors);
        }
    }
}
