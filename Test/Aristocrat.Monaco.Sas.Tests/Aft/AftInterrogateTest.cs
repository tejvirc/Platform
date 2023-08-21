namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Threading;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    /// <summary>
    ///     Contains unit tests for the AftInterrogate class
    /// </summary>
    [TestClass]
    public class AftInterrogateTest
    {
        private const int WaitTime = 1000;

        private AftInterrogate _target;
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Strict);
        private readonly Mock<IAftHistoryBuffer> _historyBuffer = new Mock<IAftHistoryBuffer>(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new AftInterrogate(_aftProvider.Object, _historyBuffer.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAftProviderTest()
        {
            _target = new AftInterrogate(null, _historyBuffer.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHistoryBufferTest()
        {
            _target = new AftInterrogate(_aftProvider.Object, null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new AftInterrogate(_aftProvider.Object, _historyBuffer.Object, null);
        }

        [TestMethod]
        public void ProcessNonZeroTransactionIndexTest()
        {
            var data = new AftResponseData
            {
                TransactionIndex = 1
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                ReceiptStatus = (byte)ReceiptStatus.ReceiptPrinted
            };

            _historyBuffer.Setup(m => m.GetHistoryEntry(It.IsAny<byte>())).Returns(response);

            var actual = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.FullTransferSuccessful, actual.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPrinted, actual.ReceiptStatus);
        }

        [TestMethod]
        public void ProcessNullCurrentTransferTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransactionIndex = 0
            };

            _aftProvider.Setup(m => m.CurrentTransfer).Returns((AftResponseData)null);
            _aftProvider.Setup(m => m.TransferAmount).Returns(0L);
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.AftTransferInterrogatePending, true)).Verifiable();

            var actual = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.NoTransferInfoAvailable, actual.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, actual.ReceiptStatus);
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void ProcessCurrentTransferTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransactionIndex = 0
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.ReceiptPending
            };

            _aftProvider.Setup(m => m.CurrentTransfer).Returns(response);
            _aftProvider.Setup(m => m.TransferAmount).Returns(0L);
            _aftProvider.Setup(x => x.CreateNewTransactionHistoryEntry()).Callback(() => waiter.Set());
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.AftTransferInterrogatePending, true)).Verifiable();

            var actual = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.TransferPending, actual.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPending, actual.ReceiptStatus);
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void ProcessTransferCompleteTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransactionIndex = 0,
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.FullTransferSuccessful,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _aftProvider.Setup(m => m.CurrentTransfer).Returns(response);
            _aftProvider.Setup(m => m.TransferAmount).Returns(123L);
            _aftProvider.Setup(x => x.CreateNewTransactionHistoryEntry()).Callback(() => waiter.Set());
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.AftTransferInterrogatePending, true)).Verifiable();
            _historyBuffer.Setup(x => x.CurrentBufferIndex).Returns(1);

            var actual = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.FullTransferSuccessful, actual.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, actual.ReceiptStatus);
            _aftProvider.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void ProcessZeroAmountTransferCompleteTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransactionIndex = 0,
                TransferStatus = AftTransferStatusCode.PartialTransferSuccessful
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.PartialTransferSuccessful,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested
            };

            _aftProvider.Setup(m => m.CurrentTransfer).Returns(response);
            _aftProvider.Setup(m => m.TransferAmount).Returns(0L);
            _aftProvider.Setup(x => x.CreateNewTransactionHistoryEntry()).Callback(() => waiter.Set());
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.AftTransferInterrogatePending, true)).Verifiable();
            _historyBuffer.Setup(x => x.CurrentBufferIndex).Returns(1);

            var actual = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.PartialTransferSuccessful, actual.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.NoReceiptRequested, actual.ReceiptStatus);
            _aftProvider.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void AckNackHandlerTest()
        {
            var waiter = new ManualResetEvent(false);
            var data = new AftResponseData
            {
                TransactionIndex = 0
            };

            var response = new AftResponseData
            {
                TransferStatus = AftTransferStatusCode.TransferPending,
                ReceiptStatus = (byte)ReceiptStatus.ReceiptPending
            };

            _aftProvider.Setup(m => m.CurrentTransfer).Returns(response);
            _aftProvider.Setup(m => m.TransferAmount).Returns(0L);
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.AftTransferInterrogatePending, true)).Verifiable();

            var actual = _target.Process(data);

            // setups for Ack handler
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.AftTransferInterrogatePending, false)).Returns(true).Verifiable();
            _propertiesManager.Setup(m => m.SetProperty(SasProperties.AftTransferInterrogatePending, false)).Verifiable();
            _aftProvider.Setup(x => x.CreateNewTransactionHistoryEntry()).Callback(() => waiter.Set()).Verifiable();

            // Invoke Ack handler and verify setups matched
            _aftProvider.Object.CurrentTransfer.Handlers.ImpliedAckHandler.Invoke();

            waiter.WaitOne(200);

            // Invoke Nack handler and verify setups matched
            _aftProvider.Object.CurrentTransfer.Handlers.ImpliedNackHandler.Invoke();

            _propertiesManager.Verify(m => m.GetProperty(SasProperties.AftTransferInterrogatePending, false), Times.Once);
            _propertiesManager.Verify(m => m.SetProperty(SasProperties.AftTransferInterrogatePending, false), Times.Exactly(2));
            _aftProvider.Verify(x => x.CreateNewTransactionHistoryEntry(), Times.Once);
        }
    }
}
