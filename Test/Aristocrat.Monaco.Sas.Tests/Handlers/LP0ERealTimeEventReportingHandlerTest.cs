namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.Handlers;

    /// <summary>
    ///     Contains tests for the LP0ERealTimeEventReportingHandler class
    /// </summary>
    [TestClass]
    public class LP0ERealTimeEventReportingHandlerTest
    {
        private LP0ERealTimeEventReportingHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP0ERealTimeEventReportingHandler();
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.EnableDisableRealTimeEventReporting));
        }

        [TestMethod]
        public void HandleTest()
        {
            EnableDisableData data = new EnableDisableData { Id = 0, Enable = true };

            var result = _target.Handle(data);

            Assert.IsNull(result);
            Assert.IsTrue(_target.Client1RteEnabled);
            Assert.IsFalse(_target.Client2RteEnabled);
        }
    }
}
