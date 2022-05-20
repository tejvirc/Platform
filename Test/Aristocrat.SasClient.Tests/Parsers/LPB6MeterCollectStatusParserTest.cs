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
    ///     Contains the tests for the LPB6MeterCollectStatusParser class
    /// </summary>
    [TestClass]
    public class LPB6MeterCollectStatusParserTest
    {
        private LPB6MeterCollectStatusParser _target;
        private Mock<ISasLongPollHandler<MeterCollectStatusData, LongPollSingleValueData<byte>>> _handler;
        private static readonly List<byte> ValidHostStatus = new List<byte> { 0x00, 0x01, 0x80 };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LPB6MeterCollectStatusParser();
            _handler = new Mock<ISasLongPollHandler<MeterCollectStatusData, LongPollSingleValueData<byte>>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.MeterCollectStatus, _target.Command);
        }

        [DataTestMethod]
        [DataRow(0x00, 5)]
        [DataRow(0x01, 6)]
        [DataRow(0x80, 7)]
        [DataRow(0xA0, 8)]
        public void ParseTest(int hostStatus, int responseStatus)
        {
            var response = new MeterCollectStatusData((MeterCollectStatus)responseStatus);
            _handler.Setup(c => c.Handle(It.IsAny<LongPollSingleValueData<byte>>())).Returns(response);

            var actual = _target.Parse(CreateCommand((byte)hostStatus)).ToList();
            var expected = CreateExpectedResponse((byte)hostStatus, (byte)responseStatus).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseNullResponseTest()
        {
            var response = (MeterCollectStatusData)null;
            _handler.Setup(c => c.Handle(It.IsAny<LongPollSingleValueData<byte>>())).Returns(response);

            var actual = _target.Parse(CreateCommand(0x00)).ToList();
            var expected = new List<byte> { TestConstants.SasAddress | TestConstants.Nack };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ParseCommandTooShortTest()
        {
            var command = new List<byte>
            {
                    TestConstants.SasAddress,
                    (byte)LongPoll.MeterCollectStatus,
                    0x01
            };

            var actual = _target.Parse(command).ToList();
            var expected = new List<byte> { TestConstants.SasAddress | TestConstants.Nack };

            CollectionAssert.AreEqual(expected, actual);
        }

        private IReadOnlyCollection<byte> CreateCommand(byte hostStatus)
        {
            return new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.MeterCollectStatus,
                0x01,
                hostStatus,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };
        }

        private IReadOnlyCollection<byte> CreateExpectedResponse(byte hostStatus, byte responseStatus)
        {
            if (ValidHostStatus.Contains(hostStatus))
            {
                return new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.MeterCollectStatus,
                    0x01,
                    responseStatus
                };
            }

            return new List<byte> { TestConstants.SasAddress | TestConstants.Nack };
        }
    }
}
