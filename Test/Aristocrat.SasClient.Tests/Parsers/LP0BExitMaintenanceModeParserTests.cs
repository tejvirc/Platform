namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    /// <summary>
    ///     Contains the tests for the LP0BExitMaintenanceModeParser class
    /// </summary>
    [TestClass]
    public class LP0BExitMaintenanceModeParserTests
    {
        private LP0BExitMaintenanceModeParser _target;
        private Mock<ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<bool>>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<bool>>>(MockBehavior.Default);
            _handler.Setup(m => m.Handle(It.Is<LongPollSingleValueData<bool>>(data => !data.Value))).Returns(new LongPollResponse());

            _target = new LP0BExitMaintenanceModeParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.ExitMaintenanceMode, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.ExitMaintenanceMode, TestConstants.FakeCrc, TestConstants.FakeCrc };
            var actual = _target.Parse(command).ToArray();
            Assert.AreEqual(command[0], actual[0]);
            _handler.VerifyAll();
        }
    }
}