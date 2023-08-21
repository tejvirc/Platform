namespace Aristocrat.Monaco.Sas.Tests.Aft
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Sas.Storage;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Monaco.Sas.Storage.Repository;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Aft;

    /// <summary>
    ///     Contains the tests for the AftHistoryBuffer class
    /// </summary>
    [TestClass]
    public class AftHistoryBufferTest
    {
        private AftHistoryBuffer _target;
        private readonly Mock<IStorageDataProvider<AftHistoryItem>> _historyProvider = new Mock<IStorageDataProvider<AftHistoryItem>>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            SetupPersistenceMocks();
            _target = new AftHistoryBuffer(_historyProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHistoryProviderTest()
        {
            _target = new AftHistoryBuffer(null);
        }

        [TestMethod]
        public void AddEntryTest()
        {
            var waiter = new AutoResetEvent(false);
            _historyProvider.Setup(x => x.Save(It.IsAny<AftHistoryItem>()))
                .Returns(Task.CompletedTask)
                .Callback(() => waiter.Set());

            var result = _target.AddEntry(new AftResponseData { TransferStatus = AftTransferStatusCode.FullTransferSuccessful });

            waiter.WaitOne(100);
            Assert.AreEqual((byte)1, result);
        }

        [TestMethod]
        public void GetHistoryEntryTest()
        {
            byte transactionId = 1;

            SetBufferAtIndex(transactionId, new AftResponseData { TransferStatus = AftTransferStatusCode.FullTransferSuccessful });
            var result = _target.GetHistoryEntry(transactionId);

            Assert.AreEqual(AftTransferStatusCode.FullTransferSuccessful, result.TransferStatus);
            Assert.AreEqual(transactionId, result.TransactionIndex);
        }

        [TestMethod]
        public void GetHistoryEntryNoDataTest()
        {
            byte transactionId = 1;
            var result = _target.GetHistoryEntry(transactionId);

            Assert.AreEqual(AftTransferStatusCode.NoTransferInfoAvailable, result.TransferStatus);
            Assert.AreEqual(transactionId, result.TransactionIndex);
        }

        [TestMethod]
        public void GetHistoryEntryRelativeTest()
        {
            byte transactionId = 0x7F;

            SetBufferAtIndex(transactionId, new AftResponseData { TransferStatus = AftTransferStatusCode.FullTransferSuccessful });
            var result = _target.GetHistoryEntry(0xFF);

            Assert.AreEqual(AftTransferStatusCode.FullTransferSuccessful, result.TransferStatus);
            Assert.AreEqual(transactionId, result.TransactionIndex);
        }

        [TestMethod]
        public void GetHistoryEntryRelativeThatWrapsTest()
        {
            byte transactionId = 0x7D;

            SetBufferAtIndex(transactionId, new AftResponseData { TransferStatus = AftTransferStatusCode.FullTransferSuccessful });
            var result = _target.GetHistoryEntry(0xFD);

            Assert.AreEqual(AftTransferStatusCode.FullTransferSuccessful, result.TransferStatus);
            Assert.AreEqual(transactionId, result.TransactionIndex);
        }

        private void SetBufferAtIndex(byte index, AftResponseData aftResponseData)
        {
            aftResponseData.TransactionIndex = index;

            // use reflection to get a reference to the history buffer
            var fieldInfo = typeof(AftHistoryBuffer).GetField("_buffer", BindingFlags.NonPublic | BindingFlags.Instance);
            var buffer = (IAftHistoryLog[])fieldInfo.GetValue(_target);

            // write the new value to the buffer
            buffer[index] = aftResponseData;
            fieldInfo.SetValue(_target, buffer);
        }

        private void SetupPersistenceMocks()
        {
            var histoyLog = Enumerable.Range(0, 128).Select(_ => new AftResponseData
            {
                TransferCode = AftTransferCode.TransferRequestPartialTransferAllowed,
                TransferStatus = AftTransferStatusCode.NoTransferInfoAvailable,
                ReceiptStatus = (byte)ReceiptStatus.NoReceiptRequested,
                CashableAmount = 678,
                RestrictedAmount = 345,
                NonRestrictedAmount = 123,
                TransferFlags = AftTransferFlags.None,
                AssetNumber = 999,
                RegistrationKey = new byte[20],
                TransactionId = "abc",
                TransactionDateTime = new DateTime(2019, 1, 1, 12, 34, 56),
                Expiration = 30,
                PoolId = 12,
                CumulativeCashableAmount = 123456,
                CumulativeRestrictedAmount = 111,
                CumulativeNonRestrictedAmount = 222,
                TransactionIndex = 4
            }).ToArray();

            _historyProvider.Setup(x => x.GetData()).Returns(new AftHistoryItem
            {
                CurrentBufferIndex = 1,
                AftHistoryLog = StorageHelpers.Serialize(histoyLog)
            });
        }
    }
}
