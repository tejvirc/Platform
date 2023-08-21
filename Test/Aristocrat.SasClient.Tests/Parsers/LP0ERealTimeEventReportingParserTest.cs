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
    ///     Contains the tests for the LP0ERealTimeEventReportingParser class
    /// </summary>
    [TestClass]
    public class LP0ERealTimeEventReportingParserTest
    {
        private LP0ERealTimeEventReportingParser _target;
        private Mock<ISasClient> _client;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _client = new Mock<ISasClient>(MockBehavior.Strict);

            _target = new LP0ERealTimeEventReportingParser(_client.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.EnableDisableRealTimeEventReporting, _target.Command);
        }

        [TestMethod]
        public void HandleEnableTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.EnableDisableRealTimeEventReporting,
                0x01,   // enable
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            _client.SetupSet(m => m.IsRealTimeEventReportingActive = true).Verifiable();
            _client.Setup(m => m.ClientNumber).Returns(0);
            var handler = new Mock<ISasLongPollHandler<LongPollResponse, EnableDisableData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<EnableDisableData>())).Returns((LongPollResponse)null);

            var actual = _target.Parse(command).ToArray();

            Assert.AreEqual(command[0], actual[0]);
            _client.Verify();
        }

        [TestMethod]
        public void HandleDisableTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.EnableDisableRealTimeEventReporting,
                0x00,   // disable
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            _client.SetupSet(m => m.IsRealTimeEventReportingActive = false).Verifiable();
            _client.Setup(m => m.ClientNumber).Returns(0);
            var handler = new Mock<ISasLongPollHandler<LongPollResponse, EnableDisableData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<EnableDisableData>())).Returns((LongPollResponse)null);

            var actual = _target.Parse(command).ToArray();

            Assert.AreEqual(command[0], actual[0]);
            _client.Verify();
        }

        [TestMethod]
        public void HandleInvalidValueTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.EnableDisableRealTimeEventReporting,
                0x03,   // invalid enable/disable value
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(command).ToArray();

            Assert.AreEqual(TestConstants.SasAddress | TestConstants.Nack, actual[0]);
        }
    }
}
