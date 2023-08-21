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

    /// <summary>
    ///     Contains unit tests for the AftTransferBonusJackpotWinFromHostToGamingMachine class
    /// </summary>
    [TestClass]
    public class AftTransferBonusJackpotWinFromHostToGamingMachineTest
    {
        private AftTransferBonusJackpotWinFromHostToGamingMachine _target;
        private readonly Mock<IBank> _bank = new Mock<IBank>();
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>();
        private readonly Mock<ISasBonusCallback> _bonus = new Mock<ISasBonusCallback>(MockBehavior.Strict);
        private readonly Mock<ITime> _time = new Mock<ITime>(MockBehavior.Strict);
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Default);

        private AftResponseData _data = new AftResponseData
        {
            TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
            TransferStatus = AftTransferStatusCode.UnexpectedError,
            ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank.Setup(m => m.Limit).Returns(0L);

            _aftProvider.Setup(x => x.TransferLimitAmount).Returns(0);
            _aftProvider.Setup(x => x.CurrentTransfer).Returns(_data);
            _aftProvider.Setup(x => x.TransactionIdUnique).Returns(false);
            _aftProvider.Setup(x => x.TransactionIdValid).Returns(false);
            _aftProvider.Setup(x => x.TransferFailure).Returns(false);
            _aftProvider.Setup(x => x.FullTransferRequested).Returns(true);
            _aftProvider.SetupErrorHandler(_data);

            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { AftBonusAllowed = true, TransferInAllowed = true });

            _target = new AftTransferBonusJackpotWinFromHostToGamingMachine(_aftProvider.Object, _bonus.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullProviderTest()
        {
            _target = new AftTransferBonusJackpotWinFromHostToGamingMachine(null, _bonus.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullBonusTest()
        {
            _target = new AftTransferBonusJackpotWinFromHostToGamingMachine(_aftProvider.Object, null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new AftTransferBonusJackpotWinFromHostToGamingMachine(_aftProvider.Object, _bonus.Object, null);
        }

        [TestMethod]
        public void ProcessBonusNotAllowedTest()
        {
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                 .Returns(new SasFeatures { AftBonusAllowed = false });

            var result = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, result.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, result.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, result.TransactionDateTime);
        }

        [TestMethod]
        public void ProcessPartialBonusNotAllowedTest()
        {
            _data.TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed;
            _aftProvider.Setup(x => x.PartialTransfersAllowed).Returns(true);

            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<object>()))
                .Returns(new SasFeatures { AftBonusAllowed = true, PartialTransferAllowed = true });

            var result = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, result.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, result.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, result.TransactionDateTime);
        }

        [TestMethod]
        public void ProcessRestrictedAmountTest()
        {
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;
            _data.RestrictedAmount = 1;

            var result = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, result.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, result.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, result.TransactionDateTime);
        }

        [TestMethod]
        public void ProcessNoReceiptTest()
        {
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;
            _data.TransferFlags = AftTransferFlags.TransactionReceiptRequested;
            _aftProvider.Setup(x => x.IsTransferFlagSet(AftTransferFlags.TransactionReceiptRequested)).Returns(true);

            var result = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.TransactionReceiptNotAllowedForTransferType, result.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, result.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, result.TransactionDateTime);
        }

        [TestMethod]
        public void DoAftBonusJackpotWinBonusNotAllowedTest()
        {
            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;
            _bonus.Setup(m => m.IsAftBonusAllowed(It.IsAny<AftData>())).Returns(false);

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineUnableToPerformTransfer, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void DoAftBonusJackpotWinAwardBonusTest()
        {
            var waiter = new ManualResetEvent(false);

            _data.TransferCode = AftTransferCode.TransferRequestFullTransferOnly;
            _bonus.Setup(m => m.IsAftBonusAllowed(It.IsAny<AftData>())).Returns(true);
            _aftProvider.Setup(x => x.DoBonus()).Returns(Task.CompletedTask).Callback(() => waiter.Set()).Verifiable();

            _target.Process(_data);

            Assert.IsTrue(waiter.WaitOne());
            _aftProvider.Verify();
        }
    }
}
