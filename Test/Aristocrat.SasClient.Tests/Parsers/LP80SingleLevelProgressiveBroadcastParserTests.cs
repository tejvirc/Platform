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
    public class LP80SingleLevelProgressiveBroadcastParserTests
    {
        private Mock<ISasLongPollHandler<LongPollAck, SingleLevelProgressiveBroadcastData>> _handler;
        private LP80SingleLevelProgressiveBroadcastParser _target;

        [TestMethod]
        public void CommandTest()
        {
            _target = new LP80SingleLevelProgressiveBroadcastParser();
            Assert.AreEqual(LongPoll.SingleLevelProgressiveBroadcastValue, _target.Command);
        }

        [TestMethod]
        public void ParseTest()
        {
            _target = new LP80SingleLevelProgressiveBroadcastParser();

            _handler =
                new Mock<ISasLongPollHandler<LongPollAck, SingleLevelProgressiveBroadcastData>>(
                    MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SingleLevelProgressiveBroadcastValue,
                0x01, //Group Id of the Progressive
                0x01, //Progressive Level
                0x00, 0x00, 0x00, 0x00, 0x08 //Win amount
            };

            var response = new LongPollAck(true);

            _handler.Setup(c => c.Handle(It.IsAny<SingleLevelProgressiveBroadcastData>())).Returns(response);
            _target.InjectHandler(_handler.Object);
            var actual = _target.Parse(command).ToList();

            var expectedResults = new List<byte> { TestConstants.SasAddress };
            CollectionAssert.AreEqual(expectedResults, actual);
        }
    }
}