namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Test.Common;

    /// <summary>
    ///     Tests for the LP7FReceiveDateAndTimeHandler class
    /// </summary>
    [TestClass]
    public class LP7FReceiveDateAndTimeHandlerTest
    {
        private LP7FReceiveDateAndTimeHandler _target;
        private Mock<ITime> _testTime = new Mock<ITime>(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP7FReceiveDateAndTimeHandler(_testTime.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.ReceiveDateTime));
        }

        [TestMethod]
        public void TestGoodTime()
        {
            var dateTime = new DateTime(2011, 12, 13, 9, 21, 59);
            var data = new LongPollDateTimeData(dateTime);

            _testTime.Setup(m => m.Update(dateTime)).Returns(true);
            var output = _target.Handle(data);
            Assert.IsTrue(output.Data);
            _testTime.Setup(m => m.Update(dateTime)).Returns(false);
            output = _target.Handle(data);
            Assert.IsFalse(output.Data);
        }

        [TestMethod]
        public void TestBadTime()
        {
            var dateTime = new DateTime(2011, 12, 13, 9, 21, 59);
            var data = new LongPollDateTimeData(dateTime);

            _testTime.Setup(m => m.Update(dateTime)).Returns(false);
            var output = _target.Handle(data);
            Assert.IsFalse(output.Data);
        }
    }
}
