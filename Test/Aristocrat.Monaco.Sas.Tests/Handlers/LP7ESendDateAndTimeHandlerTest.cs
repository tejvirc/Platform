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
    ///     Tests for the LP7ESendDateAndTimeHandlerTest class
    /// </summary>
    [TestClass]
    public class LP7ESendDateAndTimeHandlerTest
    {
        private LP7ESendDateAndTimeHandler _target;
        private Mock<ITime> _testTime;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _testTime = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);

            _target = new LP7ESendDateAndTimeHandler(_testTime.Object);
       }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SetCurrentDateTime));
        }

        [TestMethod]
        public void HandleTest()
        {
            DateTime _dateTime = new DateTime(2011, 12, 13, 9, 21, 59);
            _testTime.Setup(m => m.GetLocationTime()).Returns(_dateTime);
            var data = new LongPollDateTimeData(_dateTime);
            var expected = _target.Handle(data);

            Assert.AreEqual(_dateTime, expected.DateAndTime);
        }
    }
}
