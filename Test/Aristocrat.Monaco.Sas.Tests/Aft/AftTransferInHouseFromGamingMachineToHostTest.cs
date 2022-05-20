namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
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

    /// <summary>
    ///     Contains the unit tests for the AftTransferInHouseFromGamingMachineToHost class
    /// </summary>
    [TestClass]
    public class AftTransferInHouseFromGamingMachineToHostTest
    {
        private const int WaitTime = 1000;

        private AftTransferInHouseFromGameMachineToHost _target;
        private readonly Mock<IBank> _bank = new Mock<IBank>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        private readonly Mock<IHostCashOutProvider> _hostCashOutProvider = new Mock<IHostCashOutProvider>();
        private readonly Mock<ITime> _time = new Mock<ITime>(MockBehavior.Strict);
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Default);
        private const ulong OneDollarCents = 100ul;

        private AftResponseData _data = new AftResponseData
        {
            TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
            TransferStatus = AftTransferStatusCode.UnexpectedError
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank.Setup(m => m.Limit).Returns(100L);
            _aftProvider.Setup(x => x.TransferLimitAmount).Returns(100);
            _aftProvider.SetupErrorHandler(_data);
            _aftProvider.Setup(x => x.CurrentTransfer).Returns(_data);
            _aftProvider.Setup(x => x.TransactionIdUnique).Returns(false);
            _aftProvider.Setup(x => x.TransactionIdValid).Returns(false);
            _aftProvider.Setup(x => x.TransferFailure).Returns(false);
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(true);
            _aftProvider.Setup(x => x.PartialTransfersAllowed).Returns(false);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>())).Returns(new SasFeatures { TransferOutAllowed = true });
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);

            _target = new AftTransferInHouseFromGameMachineToHost(_aftProvider.Object, _hostCashOutProvider.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAfOverlordTest()
        {
            _target = new AftTransferInHouseFromGameMachineToHost(null, _hostCashOutProvider.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHostCashOutProviderTest()
        {
            _target = new AftTransferInHouseFromGameMachineToHost(_aftProvider.Object, null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new AftTransferInHouseFromGameMachineToHost(_aftProvider.Object, _hostCashOutProvider.Object, null);
        }

        [TestMethod]
        public void NoMoneyToTransferOffTest()
        {
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(0);
            _aftProvider.Setup(x => x.TransferAmount).Returns(1);

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferAmountOrExpirationDate, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void NotConfiguredToDoTransfersOffTest()
        {
            _bank.Setup(m => m.QueryBalance()).Returns(1L);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>())).Returns(new SasFeatures { TransferOutAllowed = false });

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void TransferMoreThanAftTransferLimitTest()
        {
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(10);
            _aftProvider.Setup(x => x.TransferAmount).Returns(101);
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.TransferAmountExceedsGameLimit, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void WinHostCashOutFailsTransfer()
        {
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(1);
            _hostCashOutProvider.Setup(x => x.CashOutWinPending).Returns(true);
            _aftProvider.Setup(x => x.TransferAmount).Returns(100);
            _data.TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed;
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(false);
            _aftProvider.Setup(x => x.PartialTransfersAllowed).Returns(true);

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineUnableToPerformTransfer, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void NotEnoughCashToTransferOffTest()
        {
            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
                CashableAmount = 10ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCentsForAccount(AccountType.Cashable)).Returns(0);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void NoEnoughPromoCreditsToTransferOffTest()
        {
            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
                NonRestrictedAmount = 10ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCentsForAccount(AccountType.Promo)).Returns(0);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void NotEnoughRestrictedCreditsToTransferOffTest()
        {
            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
                RestrictedAmount = 10ul,
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCentsForAccount(AccountType.NonCash)).Returns(0);

            var response = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
        }

        [TestMethod]
        public void DoAftOffIsAftPendingTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
                NonRestrictedAmount = 1ul,
                ReceiptData = new AftReceiptData()
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCentsForAccount(AccountType.Promo)).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.DoAftOff()).Returns(Task.CompletedTask).Callback(() => waiter.Set());

            var response = _target.Process(data);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, response.ReceiptStatus);
            Assert.AreEqual(0, response.PoolId);
        }

        [TestMethod]
        public void DoAftOffAftOffFullRequestPassTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
                TransferStatus = AftTransferStatusCode.TransferPending,
                RestrictedAmount = 1ul,
                ReceiptData = new AftReceiptData()
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCentsForAccount(AccountType.NonCash)).Returns(OneDollarCents);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { PartialTransferAllowed = true, TransferOutAllowed = true });
            _aftProvider.Setup(x => x.DoAftOff()).Returns(Task.CompletedTask).Callback(() => waiter.Set());

            var response = _target.Process(data);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, response.ReceiptStatus);
        }

        [TestMethod]
        public void DoAftOffAftOffPartialRequestPassTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransferType = AftTransferType.GameToHostInHouse,
                TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed,
                TransferStatus = AftTransferStatusCode.TransferPending,
                NonRestrictedAmount = 1ul,
                ReceiptData = new AftReceiptData()
            };

            _aftProvider.Setup(x => x.CurrentTransfer).Returns(data);
            _aftProvider.SetupErrorHandler(data);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.CurrentBankBalanceInCentsForAccount(AccountType.Promo)).Returns(OneDollarCents);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { PartialTransferAllowed = true, TransferOutAllowed = true });
            _aftProvider.Setup(x => x.DoAftOff()).Returns(Task.CompletedTask).Callback(() => waiter.Set());

            var response = _target.Process(data);

            Assert.IsTrue(waiter.WaitOne(WaitTime));
            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, response.ReceiptStatus);
        }
    }
}
