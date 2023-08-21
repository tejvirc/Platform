namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using LongPollAck = Sas.Client.LongPollDataClasses.LongPollReadSingleValueResponse<bool>;

    [TestClass]
    public class LP7AExtendedProgressiveBroadcastParserTest
    {
        private Mock<ISasLongPollHandler<LongPollAck, ExtendedProgressiveBroadcastData>> _handler;
        private LP7AExtendedProgressiveBroadcastParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP7AExtendedProgressiveBroadcastParser();
            _handler = new Mock<ISasLongPollHandler<LongPollAck, ExtendedProgressiveBroadcastData>>(
                    MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.ExtendedProgressiveBroadcast, _target.Command);
        }

        [DataTestMethod]
        [DataRow(true, new [] { TestConstants.SasAddress })]
        [DataRow(false, null)]
        public void ValidParseTest(bool ackPoll, byte[] expectedResponse)
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ExtendedProgressiveBroadcast,
                0x0F, //length
                0x01, //Group Id of the Progressive
                0x01, //Progressive Level
                0x00, 0x00, 0x00, 0x00, 0x08, //Win amount
                0x00, 0x00, 0x00, 0x00, 0x06, //Base amount
                0x00, 0x00, 0x01, //Contribution
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            _handler.Setup(c => c.Handle(It.IsAny<ExtendedProgressiveBroadcastData>())).Returns(() => new LongPollAck(ackPoll));
            _target.InjectHandler(_handler.Object);
            var actual = _target.Parse(command)?.ToList();
            CollectionAssert.AreEqual(expectedResponse, actual);
        }

        [DataTestMethod]
        [DataRow(new []
        {
            TestConstants.SasAddress,
            (byte)LongPoll.ExtendedProgressiveBroadcast
        }, DisplayName = "Not Enough bytes test")]
        [DataRow(new byte[]
        {
            TestConstants.SasAddress,
            (byte)LongPoll.ExtendedProgressiveBroadcast,
            0x1F, // length
            0x01, // Group Id of the Progressive
            0x01, // Progressive Level
            0x00, 0x00, 0x00, 0x00, 0x08, // Win amount
            0x00, 0x00, 0x00, 0x00, 0x06, // Base amount
            0x00, 0x00, 0x01, // Contribution
            TestConstants.FakeCrc, TestConstants.FakeCrc
        }, DisplayName = "Invalid Length test")]
        [DataRow(new byte[]
        {
            TestConstants.SasAddress,
            (byte)LongPoll.ExtendedProgressiveBroadcast,
            0x0F, // length
            0x01, // Group Id of the Progressive
            0x01, // Progressive Level
            0x00, 0x00, 0x00, 0x0F, 0x08, // Win amount
            0x00, 0x00, 0x00, 0x00, 0x06, // Base amount
            0x00, 0x00, 0x01, // Contribution
            TestConstants.FakeCrc, TestConstants.FakeCrc
        }, DisplayName = "Invalid Level Amount BCD test")]
        [DataRow(new byte[]
        {
            TestConstants.SasAddress,
            (byte)LongPoll.ExtendedProgressiveBroadcast,
            0x0F, // length
            0x01, // Group Id of the Progressive
            0x01, // Progressive Level
            0x00, 0x00, 0x00, 0x00, 0x08, // Win amount
            0x00, 0x00, 0x00, 0x0F, 0x06, // Base amount
            0x00, 0x00, 0x01, // Contribution
            TestConstants.FakeCrc, TestConstants.FakeCrc
        }, DisplayName = "Invalid Base Amount test")]
        [DataRow(new byte[]
        {
            TestConstants.SasAddress,
            (byte)LongPoll.ExtendedProgressiveBroadcast,
            0x0F, // length
            0x01, // Group Id of the Progressive
            0x01, // Progressive Level
            0x00, 0x00, 0x00, 0x00, 0x08, // Win amount
            0x00, 0x00, 0x00, 0x00, 0x06, // Base amount
            0x00, 0x00, 0xF0, // Contribution
            TestConstants.FakeCrc, TestConstants.FakeCrc
        }, DisplayName = "Invalid Contribution test")]
        public void InvalidDataParseTest(byte[] command)
        {
            var actual = _target.Parse(command).ToList();
            var expectedResults = new List<byte> { TestConstants.SasAddress | SasConstants.Nack };
            CollectionAssert.AreEqual(expectedResults, actual);
        }
    }
}