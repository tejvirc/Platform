namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;
    using Sas.AftTransferProvider;

    [TestClass]
    public class AftTransferWinAmountFromGameMachineToHostTest
    {
        private AftTransferWinAmountFromGameMachineToHost _target;
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        private readonly Mock<IHostCashOutProvider> _hostCashOutProvider = new Mock<IHostCashOutProvider>();
        private readonly Mock<ITime> _time = new Mock<ITime>(MockBehavior.Strict);
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Default);

        private readonly AftResponseData _data = new AftResponseData
        {
            TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
            TransferStatus = AftTransferStatusCode.UnexpectedError
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(0);
            _aftProvider.Setup(x => x.CurrentTransfer).Returns(_data);
            _aftProvider.Setup(x => x.TransactionIdUnique).Returns(false);
            _aftProvider.Setup(x => x.TransactionIdValid).Returns(false);
            _aftProvider.Setup(x => x.TransferLimitAmount).Returns(1_000ul);
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(true);
            _aftProvider.SetupErrorHandler(_data);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { WinTransferAllowed = true, TransferOutAllowed = true });

            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);

            _target = new AftTransferWinAmountFromGameMachineToHost(_aftProvider.Object, _hostCashOutProvider.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAfOverlordTest()
        {
            _target = new AftTransferWinAmountFromGameMachineToHost(null, _hostCashOutProvider.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHostCashOutProviderTest()
        {
            _target = new AftTransferWinAmountFromGameMachineToHost(_aftProvider.Object, null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new AftTransferWinAmountFromGameMachineToHost(_aftProvider.Object, _hostCashOutProvider.Object, null);
        }

        [TestMethod]
        public void NotConfiguredToDoTransfersOffTest()
        {
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { WinTransferAllowed = false, TransferOutAllowed = true });

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void TransferMoreThanAftTransferLimitTest()
        {
            _hostCashOutProvider.Setup(m => m.CashOutTransaction).Returns(new WatTransaction() { CashableAmount = 1001 });
            _hostCashOutProvider.Setup(m => m.CashOutWinPending).Returns(true);
            _hostCashOutProvider.Setup(m => m.CanCashOut).Returns(true);
            _aftProvider.Setup(x => x.TransferAmount).Returns(1_001ul);  // over the limit
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.TransferAmountExceedsGameLimit, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void NoAvailableWinTestTransfer()
        {
            _hostCashOutProvider.Setup(m => m.CanCashOut).Returns(true);
            _hostCashOutProvider.Setup(x => x.CashOutWinPending).Returns(false);
            _aftProvider.Setup(x => x.TransferAmount).Returns(10ul);
            _aftProvider.Setup(x => x.PartialTransfersAllowed).Returns(true);
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(false);
            _data.TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed;

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NoWonCreditsAvailableForCashOut, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void NotEnoughCashToTransferOffTest()
        {
            _hostCashOutProvider.Setup(m => m.CashOutTransaction).Returns(new WatTransaction() { CashableAmount = 1000 });
            _hostCashOutProvider.Setup(m => m.CashOutWinPending).Returns(true);
            _hostCashOutProvider.Setup(m => m.CanCashOut).Returns(true);
            _aftProvider.Setup(x => x.TransferAmount).Returns(10ul);
            _data.CashableAmount = 10ul;
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;
            _data.TransferStatus = AftTransferStatusCode.FullTransferSuccessful;

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
        }

        [TestMethod]
        public void NoEnoughPromoCreditsToTransferOffTest()
        {
            _hostCashOutProvider.Setup(m => m.CashOutTransaction).Returns(new WatTransaction() { CashableAmount = 1000 });
            _hostCashOutProvider.Setup(m => m.CashOutWinPending).Returns(true);
            _hostCashOutProvider.Setup(m => m.CanCashOut).Returns(true);
            _aftProvider.Setup(x => x.TransferAmount).Returns(10ul);
            _data.NonRestrictedAmount = 10ul;
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
        }

        [TestMethod]
        public void NotEnoughRestrictedCreditsToTransferOffTest()
        {
            _hostCashOutProvider.Setup(m => m.CashOutTransaction).Returns(new WatTransaction() { CashableAmount = 1000 });
            _hostCashOutProvider.Setup(m => m.CashOutWinPending).Returns(true);
            _hostCashOutProvider.Setup(m => m.CanCashOut).Returns(true);
            _aftProvider.Setup(x => x.TransferAmount).Returns(10ul);
            _data.RestrictedAmount = 10ul;
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void DoAftOffTest()
        {
            const int waitTime = 1000;
            var waiter = new ManualResetEvent(false);

            _hostCashOutProvider.Setup(m => m.CashOutWinPending).Returns(true);
            _hostCashOutProvider.Setup(m => m.CanCashOut).Returns(true);
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;
            _data.TransferType = AftTransferType.GameToHostInHouseWin;
            _data.ReceiptData = new AftReceiptData();

            _aftProvider.Setup(x => x.TransferAmount).Returns(10ul);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { WinTransferAllowed = true, TransferOutAllowed = true, PartialTransferAllowed = false });
            _aftProvider.Setup(m => m.DoAftOff()).Returns(Task.CompletedTask).Callback(() => waiter.Set());

            var response = _target.Process(_data);

            Assert.IsTrue(waiter.WaitOne(waitTime));
            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, response.ReceiptStatus);
        }
    }
}
