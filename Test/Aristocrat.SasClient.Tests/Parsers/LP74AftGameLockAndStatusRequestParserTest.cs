namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.Aft;
    using Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Contains tests for the LP74AftGameLockAndStatusRequestParser class
    /// </summary>
    [TestClass]
    public class LP74AftGameLockAndStatusRequestParserTest
    {
        private readonly Mock<ISasLongPollHandler<AftGameLockAndStatusResponseData, AftGameLockAndStatusData>> _handler = new Mock<ISasLongPollHandler<AftGameLockAndStatusResponseData, AftGameLockAndStatusData>>();
        private LP74AftGameLockAndStatusRequestParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP74AftGameLockAndStatusRequestParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void ParseWithInterrogateOnlyTest()
        {
            AftGameLockAndStatusResponseData response = new AftGameLockAndStatusResponseData
            {
                AssetNumber = 0x12345678,
                GameLockStatus = AftGameLockStatus.GameNotLocked,
                AvailableTransfers = AftAvailableTransfers.TransferToGamingMachineOk,
                HostCashoutStatus = AftTransferFlags.HostCashOutEnable,
                AftStatus = AftStatus.AnyAftEnabled,
                MaxBufferIndex = 0x7F,
                CurrentCashableAmount = 9876543,
                CurrentRestrictedAmount = 8765432,
                CurrentNonRestrictedAmount = 7654321,
                CurrentGamingMachineTransferLimit = 1000,
                RestrictedExpiration = 30,
                RestrictedPoolId = 0x0C00
            };
            _handler.Setup(x => x.Handle(It.IsAny<AftGameLockAndStatusData>())).Returns(response);

            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x74,
                (byte)AftLockCode.InterrogateCurrentStatusOnly, 0x00,
                0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress, 0x74, 0x23,
                0x78, 0x56, 0x34, 0x12,  // Asset number
                0xFF,  // game not locked
                0x01,  // available transfers
                0x02,  // Host cashout status
                0x80,  // AFT status
                0x7F,  // max buffer index
                0x00, 0x09, 0x87, 0x65, 0x43, // cashable amount
                0x00, 0x08, 0x76, 0x54, 0x32, // restricted amount
                0x00, 0x07, 0x65, 0x43, 0x21, // non-restricted amount
                0x00, 0x00, 0x00, 0x10, 0x00, // transfer limit
                0x00, 0x00, 0x00, 0x30,   // restricted expiration
                0x00, 0x0C  // pool id
            };

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWithCancelLockTest()
        {
            AftGameLockAndStatusResponseData response = new AftGameLockAndStatusResponseData();
            _handler.Setup(x => x.Handle(It.IsAny<AftGameLockAndStatusData>())).Returns(response);

            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x74,
                (byte)AftLockCode.CancelLockOrPendingLockRequest, 0x00,
                0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress, 0x74, 0x23,
                0,0,0,0,0,0,0,0,0,0, // 0x23 more bytes
                0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0
            };

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWithValidLockTest()
        {
            AftGameLockAndStatusResponseData response = new AftGameLockAndStatusResponseData();
            _handler.Setup(x => x.Handle(It.IsAny<AftGameLockAndStatusData>())).Returns(response);

            byte goodBcd = 0x99;
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x74,
                (byte)AftLockCode.RequestLock, 0x00,
                goodBcd, goodBcd,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress, 0x74, 0x23,
                0,0,0,0,0,0,0,0,0,0, // 0x23 more bytes
                0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0,0,0,0,0,0,
                0,0,0,0,0
            };

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseWithInvalidLockTimeoutTest()
        {
            byte badBcd = 0xAB;
            var longPoll = new Collection<byte>
            {
                TestConstants.SasAddress, 0x74,
                (byte)AftLockCode.RequestLock, 0x00,
                badBcd, badBcd,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var actual = _target.Parse(longPoll).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
