namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Ticketing;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    /// <summary>
    ///     Contains the unit tests for the AftTransferInHouseFromHostToGameMachine class
    /// </summary>
    [TestClass]
    public class AftTransferInHouseFromHostToGameMachineTest
    {
        private AftTransferInHouseFromHostToGameMachine _target;
        private readonly Mock<IBank> _bank = new Mock<IBank>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        private readonly Mock<ITicketingCoordinator> _ticketingCoordinator = new Mock<ITicketingCoordinator>(MockBehavior.Strict);
        private readonly Mock<ITime> _time = new Mock<ITime>(MockBehavior.Strict);
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Default);

        private const long OneDollarMillicents = 100_000L;
        private const ulong OneDollarCents = 100ul;
        private const int SignalWaitTimeOut = 100;  // time to wait for a signal that Task->Run is done

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank.Setup(m => m.Limit).Returns(OneDollarMillicents); // credit limit of $1.00
            _aftProvider.Setup(x => x.TransferLimitAmount).Returns(OneDollarCents);

            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { PartialTransferAllowed = true, TransferInAllowed = true });

            _target = new AftTransferInHouseFromHostToGameMachine(_aftProvider.Object, _ticketingCoordinator.Object, _bank.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAftProviderTest()
        {
            _target = new AftTransferInHouseFromHostToGameMachine(null, _ticketingCoordinator.Object, _bank.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTicketingCoordinatorTest()
        {
            _target = new AftTransferInHouseFromHostToGameMachine(_aftProvider.Object, null, _bank.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullBankTest()
        {
            _target = new AftTransferInHouseFromHostToGameMachine(_aftProvider.Object, _ticketingCoordinator.Object, null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new AftTransferInHouseFromHostToGameMachine(_aftProvider.Object, _ticketingCoordinator.Object, _bank.Object, null);
        }

        [TestMethod]
        public void TransferOverCreditLimitTest()
        {
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(200);
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(true);
            _aftProvider.SetupErrorHandler(data);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.TransferAmountExceedsGameLimit, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void TransferOverCreditLimitPartialTransferTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed,
                TransferStatus = AftTransferStatusCode.TransferPending,
                RestrictedAmount = 1ul,
                ReceiptData = new AftReceiptData()
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.Setup(x => x.PartialTransfersAllowed).Returns(true);
            _aftProvider.Setup(x => x.TransferAmount).Returns(1);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(OneDollarCents); // Current balance is at max

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.TransferAmountExceedsGameLimit, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void RestrictedCreditsHaveSamePoolIdTest()
        {
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
                RestrictedAmount = 10,  // 10 cents
                PoolId = 345
            };

            _ticketingCoordinator.Setup(x => x.GetData()).Returns(new TicketStorageData { PoolId = 123 });
            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _bank.Setup(m => m.QueryBalance(AccountType.NonCash)).Returns(1L);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.UnableToAcceptTransferDueToExistingRestrictedAmounts, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void NotConfiguredForTransfersOnTest()
        {
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { PartialTransferAllowed = true, TransferInAllowed = false });

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void OverMaxAftTransferTest()
        {
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(true);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents + OneDollarCents);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.TransferAmountExceedsGameLimit, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void DoAftOnIsAftPendingTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _aftProvider.Setup(x => x.DoAftOn()).Returns(Task.CompletedTask).Callback(() => waiter.Set());

            var response = _target.Process(data);

            Assert.IsTrue(waiter.WaitOne(SignalWaitTimeOut));
            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, response.ReceiptStatus);
            Assert.AreEqual(0, response.PoolId);
        }

        [TestMethod]
        public void DoAftOnAftOnFullRequestPassTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
                RestrictedAmount = 1ul,
                ReceiptData = new AftReceiptData()
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { PartialTransferAllowed = true, TransferInAllowed = true });
            _aftProvider.Setup(x => x.DoAftOn()).Returns(Task.CompletedTask).Callback(() => waiter.Set()); ;

            var response = _target.Process(data);

            Assert.IsTrue(waiter.WaitOne(SignalWaitTimeOut));
            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, response.ReceiptStatus);
        }

        [TestMethod]
        public void DoAftOnAftOnPartialRequestPassTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransferType = AftTransferType.HostToGameInHouse,
                TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed,
                TransferStatus = AftTransferStatusCode.TransferPending,
                RestrictedAmount = 1ul,
                ReceiptData = new AftReceiptData()
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _bank.Setup(m => m.QueryBalance()).Returns(0L);
            _aftProvider.Setup(x => x.DoAftOn()).Returns(Task.CompletedTask).Callback(() => waiter.Set());

            var response = _target.Process(data);

            Assert.IsTrue(waiter.WaitOne(SignalWaitTimeOut));
            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, response.ReceiptStatus);
        }
    }
}
