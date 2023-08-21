namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    /// <summary>
    ///     Contains unit tests for the AftInterrogateStatusOnly class
    /// </summary>
    [TestClass]
    public class AftInterrogateStatusOnlyTest
    {
        private AftInterrogateStatusOnly _target;
        private readonly Mock<IAftTransferProvider> _aftProvider = new Mock<IAftTransferProvider>(MockBehavior.Strict);
        private readonly Mock<IAftHistoryBuffer> _historyBuffer = new Mock<IAftHistoryBuffer>(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new AftInterrogateStatusOnly(_aftProvider.Object, _historyBuffer.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullAftProviderTest()
        {
            _target = new AftInterrogateStatusOnly(null, _historyBuffer.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHistoryBufferTest()
        {
            _target = new AftInterrogateStatusOnly(_aftProvider.Object, null);
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
        public void ProcessCurrentTransferTest()
        {
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

            var actual = _target.Process(data);

            Assert.AreEqual(AftTransferStatusCode.TransferPending, actual.TransferStatus);
            Assert.AreEqual((byte)ReceiptStatus.ReceiptPending, actual.ReceiptStatus);
        }
    }
}
