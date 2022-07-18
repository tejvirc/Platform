namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;
    using Sas.AftTransferProvider;

    /// <summary>
    ///     Contains tests for the LP72AftTransferFundsHandler class
    /// </summary>
    [TestClass]
    public class LP72AftTransferFundsHandlerTest
    {
        private const ulong AftOffLimit = 1000ul;

        private LP72AftTransferFundsHandler _target;
        private readonly Mock<IBank> _bank = new Mock<IBank>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        private readonly Mock<IAftRegistrationProvider> _aftRegistrationProvider = new Mock<IAftRegistrationProvider>();
        private readonly Mock<IAftHistoryBuffer> _historyBuffer = new Mock<IAftHistoryBuffer>(MockBehavior.Strict);
        private readonly Mock<IFundsTransferDisable> _fundsTransferDisable = new Mock<IFundsTransferDisable>(MockBehavior.Strict);
        private readonly Mock<IAutoPlayStatusProvider> _autoPlayStatusProvider = new Mock<IAutoPlayStatusProvider>(MockBehavior.Strict);
        private readonly Mock<ITicketingCoordinator> _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Default);
        private Mock<IAftHostCashOutProvider> _hostCashoutProvider;
        private Mock<ISasBonusCallback> _bonus;

        private Mock<IAftTransferProvider> _aftProvider;
        private Mock<IAftRequestProcessorTransferCode> _aftTransferFullPartial;
        private Mock<IAftRequestProcessorTransferCode> _aftInterrogate;
        private Mock<IAftRequestProcessorTransferCode> _aftInterrogateStatusOnly;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank.Setup(m => m.Limit).Returns(0L);

            _bonus = new Mock<ISasBonusCallback>(MockBehavior.Default);

            _aftProvider = new Mock<IAftTransferProvider>();
            _hostCashoutProvider = new Mock<IAftHostCashOutProvider>(MockBehavior.Default);
            _aftProvider.Setup(x => x.TransferLimitAmount).Returns(AftOffLimit);

            var aftTransferBonusCoinOutWinFromHostToGamingMachine = new AftTransferBonusCoinOutWinFromHostToGamingMachine(_aftProvider.Object, _bonus.Object, _propertiesManager.Object);
            var aftTransferBonusJackpotWinFromHostToGamingMachine = new AftTransferBonusJackpotWinFromHostToGamingMachine(_aftProvider.Object, _bonus.Object, _propertiesManager.Object);
            var aftTransferDebitFromHostToGamingMachine = new AftTransferDebitFromHostToGamingMachine(_aftProvider.Object, _aftRegistrationProvider.Object);
            var aftTransferDebitFromHostToTicket = new AftTransferDebitFromHostToTicket(_aftProvider.Object, _aftRegistrationProvider.Object);
            var aftTransferInHouseFromGameMachineToHost = new AftTransferInHouseFromGameMachineToHost(_aftProvider.Object, _hostCashoutProvider.Object, _propertiesManager.Object);
            var aftTransferInHouseFromHostToGameMachine = new AftTransferInHouseFromHostToGameMachine(_aftProvider.Object, _ticketingCoordinator.Object, _bank.Object, _propertiesManager.Object);
            var aftTransferWinAmountFromGameMachineToHost = new AftTransferWinAmountFromGameMachineToHost(_aftProvider.Object, _hostCashoutProvider.Object, _propertiesManager.Object);
            var aftTransferInHouseFromHostToTicket = new AftTransferInHouseFromHostToTicket(_aftProvider.Object);

            IEnumerable<IAftRequestProcessor> aftFullProcessors = new List<IAftRequestProcessor>
            {
                aftTransferBonusCoinOutWinFromHostToGamingMachine,
                aftTransferBonusJackpotWinFromHostToGamingMachine,
                aftTransferDebitFromHostToGamingMachine,
                aftTransferDebitFromHostToTicket,
                aftTransferInHouseFromGameMachineToHost,
                aftTransferInHouseFromHostToGameMachine,
                aftTransferInHouseFromHostToTicket,
                aftTransferWinAmountFromGameMachineToHost
            };

            _aftTransferFullPartial = new Mock<AftTransferFullPartial>(
                _aftProvider.Object,
                _hostCashoutProvider.Object,
                _fundsTransferDisable.Object,
                _autoPlayStatusProvider.Object,
                _aftRegistrationProvider.Object,
                aftFullProcessors).As<IAftRequestProcessorTransferCode>();
            _aftInterrogate =
                new Mock<AftInterrogate>(_aftProvider.Object, _historyBuffer.Object, _propertiesManager.Object)
                    .As<IAftRequestProcessorTransferCode>();
            _aftInterrogateStatusOnly = new Mock<AftInterrogateStatusOnly>(_aftProvider.Object, _historyBuffer.Object)
                .As<IAftRequestProcessorTransferCode>();

            var aftRequestProcessors = new List<IAftRequestProcessorTransferCode>
            {
                _aftTransferFullPartial.Object,
                _aftInterrogate.Object,
                _aftInterrogateStatusOnly.Object
            };

            _target = new LP72AftTransferFundsHandler(_aftProvider.Object, aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAftProviderTest()
        {
            IEnumerable<IAftRequestProcessorTransferCode> aftRequestProcessors = new List<IAftRequestProcessorTransferCode>
            {
                _aftTransferFullPartial.Object,
                _aftInterrogate.Object,
                _aftInterrogateStatusOnly.Object
            };

            _target = new LP72AftTransferFundsHandler(null, aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingAftTransferFullPartialTest()
        {
            IEnumerable<IAftRequestProcessorTransferCode> aftRequestProcessors = new List<IAftRequestProcessorTransferCode>
            {
                _aftInterrogate.Object,
                _aftInterrogateStatusOnly.Object
            };

            _target = new LP72AftTransferFundsHandler(_aftProvider.Object, aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingAftInterrogateTest()
        {
            IEnumerable<IAftRequestProcessorTransferCode> aftRequestProcessors = new List<IAftRequestProcessorTransferCode>
            {
                _aftTransferFullPartial.Object,
                _aftInterrogateStatusOnly.Object
            };

            _target = new LP72AftTransferFundsHandler(_aftProvider.Object, aftRequestProcessors);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorMissingAftInterrogateStatusOnlyTest()
        {
            IEnumerable<IAftRequestProcessorTransferCode> aftRequestProcessors = new List<IAftRequestProcessorTransferCode>
            {
                _aftTransferFullPartial.Object,
                _aftInterrogate.Object,
            };

            _target = new LP72AftTransferFundsHandler(_aftProvider.Object, aftRequestProcessors);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.AftTransferFunds));
        }

        [TestMethod]
        public void HandleHandlerNotFoundTest()
        {
            // request invalid transfer code so handler won't be found
            var data = new AftTransferData { TransferCode = (AftTransferCode)0x03 };

            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.UnsupportedTransferCode,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            var response = _target.Handle(data);

            Assert.AreEqual(expected.TransferStatus, response.TransferStatus);
            Assert.AreEqual(expected.ReceiptStatus, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleCancelTest()
        {
            var data = new AftTransferData { TransferCode = AftTransferCode.CancelTransferRequest };
            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            // cancel requests should be re-routed to interrogate requests
            _aftInterrogateStatusOnly.Setup(m => m.Process(It.IsAny<AftResponseData>())).Returns(expected);

            var response = _target.Handle(data);

            Assert.AreEqual(expected.TransferStatus, response.TransferStatus);
            Assert.AreEqual(expected.ReceiptStatus, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleTransferAlreadyInProgressTest()
        {
            var data = new AftTransferData { TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed };

            _aftProvider.Setup(m => m.IsTransferAcknowledgedByHost).Returns(false);
            _aftProvider.Setup(m => m.TransferFundsRequest(It.IsAny<AftTransferData>())).Returns(true);

            var response = _target.Handle(data);

            Assert.AreEqual(AftTransferStatusCode.NotCompatibleWithCurrentTransfer, response.TransferStatus);
        }

        [TestMethod]
        public void HandleUpdateHostCashoutFlagsTest()
        {
            var data = new AftTransferData { TransferCode = AftTransferCode.TransferRequestFullTransferOnly };

            _aftProvider.Setup(m => m.IsTransferAcknowledgedByHost).Returns(true);
            _aftProvider.Setup(m => m.TransferFundsRequest(It.IsAny<AftTransferData>())).Returns(true);
            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _aftTransferFullPartial.Setup(m => m.Process(It.IsAny<AftResponseData>())).Returns(expected);

            var response = _target.Handle(data);

            Assert.AreEqual(expected.TransferStatus, response.TransferStatus);
            Assert.AreEqual(expected.ReceiptStatus, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleInterrogateTest()
        {
            var data = new AftTransferData { TransferCode = AftTransferCode.InterrogationRequest };
            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _aftInterrogate.Setup(m => m.Process(It.IsAny<AftResponseData>())).Returns(expected);

            var response = _target.Handle(data);

            Assert.AreEqual(expected.TransferStatus, response.TransferStatus);
            Assert.AreEqual(expected.ReceiptStatus, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleInterrogateStatusOnlyTest()
        {
            var data = new AftTransferData { TransferCode = AftTransferCode.InterrogationRequestStatusOnly };
            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _aftInterrogateStatusOnly.Setup(m => m.Process(It.IsAny<AftResponseData>())).Returns(expected);

            var response = _target.Handle(data);

            Assert.AreEqual(expected.TransferStatus, response.TransferStatus);
            Assert.AreEqual(expected.ReceiptStatus, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleFullTransferOnlyTest()
        {
            var data = new AftTransferData { TransferCode = AftTransferCode.TransferRequestFullTransferOnly };
            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _aftTransferFullPartial.Setup(m => m.Process(It.IsAny<AftResponseData>())).Returns(expected);

            var response = _target.Handle(data);

            Assert.AreEqual(expected.TransferStatus, response.TransferStatus);
            Assert.AreEqual(expected.ReceiptStatus, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandlePartialTransferAllowedTest()
        {
            var data = new AftTransferData { TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed };
            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _aftTransferFullPartial.Setup(m => m.Process(It.IsAny<AftResponseData>())).Returns(expected);

            var response = _target.Handle(data);

            Assert.AreEqual(expected.TransferStatus, response.TransferStatus);
            Assert.AreEqual(expected.ReceiptStatus, response.ReceiptStatus);
        }

        [TestMethod]
        public void HandleDuplicateTransferTest()
        {
            var data = new AftTransferData
            {
                TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed,
                CashableAmount = 100ul,
                TransactionId = "abc"
            };

            var expected = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested,
                CashableAmount = 100ul,
                TransactionId = "abc"
            };

            _aftProvider.SetupGet(m => m.CurrentTransfer).Returns(data.ToAftResponseData());
            _aftInterrogateStatusOnly.Setup(m => m.Process(It.IsAny<AftResponseData>())).Returns(expected);

            var response = _target.Handle(data);

            Assert.AreEqual(expected.TransferStatus, response.TransferStatus);
            Assert.AreEqual(expected.ReceiptStatus, response.ReceiptStatus);
        }
    }
}
