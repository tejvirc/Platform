namespace Aristocrat.SasClient.Tests.Parsers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Aristocrat.Sas.Client.LPParsers;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Contains the tests for the LP0AEnterMaintenanceModeParser class
    /// </summary>
    [TestClass]
    public class LP0AEnterMaintenanceModeParserTests
    {
        private LP0AEnterMaintenanceModeParser _target;
        private Mock<ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<bool>>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<LongPollResponse, LongPollSingleValueData<bool>>>(MockBehavior.Default);
            _handler.Setup(m => m.Handle(It.Is<LongPollSingleValueData<bool>>(data => data.Value))).Returns(new LongPollResponse());

            _target = new LP0AEnterMaintenanceModeParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.EnterMaintenanceMode, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.EnterMaintenanceMode, TestConstants.FakeCrc, TestConstants.FakeCrc };
            var actual = _target.Parse(command).ToArray();
            Assert.AreEqual(command[0], actual[0]);
            _handler.VerifyAll();
        }
    }
}