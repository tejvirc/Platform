namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    ///     Contains tests for LPB6MeterCollectStatusHandler
    /// </summary>
    [TestClass]
    public class LPB6MeterCollectStatusHandlerTest
    {
        private const MeterCollectStatus ExpectedStatus = MeterCollectStatus.GameDenomPaytableChange;
        private LPB6MeterCollectStatusHandler _target;
        private Mock<ISasMeterChangeHandler> _meterChangeHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _meterChangeHandler = new Mock<ISasMeterChangeHandler>(MockBehavior.Strict);
            _meterChangeHandler.Setup(m => m.AcknowledgePendingChange());
            _meterChangeHandler.Setup(m => m.ReadyForPendingChange());
            _meterChangeHandler.Setup(m => m.CancelPendingChange());
            _meterChangeHandler.Setup(m => m.MeterChangeStatus).Returns(ExpectedStatus);

            _target = new LPB6MeterCollectStatusHandler(_meterChangeHandler.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.MeterCollectStatus));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullMeterChangeHandlerTest()
        {
            _target = new LPB6MeterCollectStatusHandler(null);
        }

        [TestMethod]
        public void DefaultConstructorTest()
        {
            var actual = new MeterCollectStatusData();
            Assert.AreEqual(MeterCollectStatus.NotInPendingChange, actual.Status);
        }

        [DataTestMethod]
        [DataRow(0x00, DisplayName = "Test for handling host acknowledgement status")]
        [DataRow(0x01, DisplayName = "Test for handling host ready status")]
        [DataRow(0x80, DisplayName = "Test for handling host unable to collect meter status")]
        public void HandleTest(int status)
        {
            var data = new LongPollSingleValueData<byte> { Value = (byte)status };

            var actual = _target.Handle(data);

            Assert.AreEqual(ExpectedStatus, actual.Status);
        }

        [TestMethod]
        public void HandleNullTest()
        {
            var data = new LongPollSingleValueData<byte> { Value = 0xA0 };

            var actual = _target.Handle(data);

            Assert.IsNull(actual);
        }
    }
}
