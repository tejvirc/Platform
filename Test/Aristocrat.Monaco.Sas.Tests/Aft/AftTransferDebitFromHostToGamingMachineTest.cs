namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    /// <summary>
    ///     This class contains the unit tests for the AftTransferDebitFromHostToGamingMachine class.
    /// </summary>
    [TestClass]
    public class AftTransferDebitFromHostToGamingMachineTest
    {
        private AftTransferDebitFromHostToGamingMachine _target;
        private readonly Mock<IAftRegistrationProvider> _registrationProvider = new Mock<IAftRegistrationProvider>(MockBehavior.Strict);
        private readonly Mock<ITime> _time = new Mock<ITime>(MockBehavior.Strict);
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Default);
        private readonly AftResponseData _data = new AftResponseData
        {
            TransferType = AftTransferType.HostToGameDebit,
            TransferCode = AftTransferCode.TransferRequestFullTransferOnly,
            TransferStatus = AftTransferStatusCode.TransferPending,
            CashableAmount = OneDollarCents,
            ReceiptData = new AftReceiptData()
        };

        private const ulong OneDollarCents = 100ul;
        private const int SignalWaitTimeOut = 100;  // time to wait for a signal that Task->Run is done

        [TestInitialize]
        public void MyTestInitialize()
        {
            _aftProvider.Setup(x => x.CurrentBankBalanceInCents).Returns(0);
            _aftProvider.Setup(x => x.CurrentTransfer).Returns(_data);
            _aftProvider.Setup(x => x.TransactionIdUnique).Returns(false);
            _aftProvider.Setup(x => x.TransactionIdValid).Returns(false);
            _aftProvider.Setup(x => x.TransferFailure).Returns(false);
            _aftProvider.Setup(x => x.TransferLimitAmount).Returns(1_000ul);
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.MaxValue);
            _aftProvider.SetupErrorHandler(_data);

            _target = new AftTransferDebitFromHostToGamingMachine(_aftProvider.Object, _registrationProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAftProviderTest()
        {
            _target = new AftTransferDebitFromHostToGamingMachine(null, _registrationProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullRegistrationProviderTest()
        {
            _target = new AftTransferDebitFromHostToGamingMachine(_aftProvider.Object, null);
        }

        [TestMethod]
        public void DebitTransfersNotSupportedTest()
        {
            _registrationProvider.Setup(m => m.IsAftDebitTransferEnabled).Returns(false);

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void GameMachineRegisteredTest()
        {
            _registrationProvider.Setup(m => m.IsAftDebitTransferEnabled).Returns(true);
            _registrationProvider.Setup(m => m.IsAftRegistered).Returns(false);

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.GamingMachineNotRegistered, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void RegistrationKeyMatchesTest()
        {
            _registrationProvider.Setup(m => m.IsAftDebitTransferEnabled).Returns(true);
            _registrationProvider.Setup(m => m.IsAftRegistered).Returns(true);
            _registrationProvider.Setup(m => m.RegistrationKeyMatches(It.IsAny<byte[]>())).Returns(false);

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.RegistrationKeyDoesNotMatch, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void PosIdZeroTest()
        {
            _registrationProvider.Setup(m => m.IsAftDebitTransferEnabled).Returns(true);
            _registrationProvider.Setup(m => m.IsAftRegistered).Returns(true);
            _registrationProvider.Setup(m => m.RegistrationKeyMatches(It.IsAny<byte[]>())).Returns(true);
            _aftProvider.Setup(x => x.PosIdZero).Returns(true);

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NoPosId, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void CashableTransferTest()
        {
            _registrationProvider.Setup(m => m.IsAftDebitTransferEnabled).Returns(true);
            _registrationProvider.Setup(m => m.IsAftRegistered).Returns(true);
            _registrationProvider.Setup(m => m.RegistrationKeyMatches(It.IsAny<byte[]>())).Returns(true);
            _aftProvider.Setup(x => x.PosIdZero).Returns(false);
            _data.RestrictedAmount = OneDollarCents;

            var response = _target.Process(_data);

            Assert.AreEqual(AftTransferStatusCode.NotAValidTransferFunction, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, response.ReceiptStatus);
            Assert.AreEqual(DateTime.MaxValue, response.TransactionDateTime);
        }

        [TestMethod]
        public void DoAftOnAftOnFullRequestPassTest()
        {
            var waiter = new ManualResetEvent(false);
            _aftProvider.Setup(x => x.TransferAmount).Returns(OneDollarCents);
            _aftProvider.Setup(x => x.PosIdZero).Returns(false);
            _registrationProvider.Setup(m => m.IsAftDebitTransferEnabled).Returns(true);
            _registrationProvider.Setup(m => m.IsAftRegistered).Returns(true);
            _registrationProvider.Setup(m => m.RegistrationKeyMatches(It.IsAny<byte[]>())).Returns(true);
            _aftProvider.Setup(m => m.DoAftOn()).Returns(Task.CompletedTask).Callback(() => waiter.Set());

            var response = _target.Process(_data);

            Assert.IsTrue(waiter.WaitOne(SignalWaitTimeOut));
            Assert.AreEqual(AftTransferStatusCode.TransferPending, response.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, response.ReceiptStatus);
        }
    }
}
