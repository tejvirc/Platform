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
    public class LP86MultipleLevelProgressiveBroadcastParserTest
    {
        private Mock<ISasLongPollHandler<LongPollAck, MultipleLevelProgressiveBroadcastData>> _handler;

        private LP86MultipleLevelProgressiveBroadcastParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP86MultipleLevelProgressiveBroadcastParser();
            _handler = new Mock<ISasLongPollHandler<LongPollAck, MultipleLevelProgressiveBroadcastData>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.MultipleLevelProgressiveBroadcastValues, _target.Command);
        }

        [TestMethod]
        public void ParseTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.MultipleLevelProgressiveBroadcastValues,
                0x07, //length
                0x01, //Group Id of the Progressive
                0x01, //Progressive Level
                0x00, 0x00, 0x00, 0x00, 0x08, //Win amount
                0x02, //Progressive Level
                0x00, 0x00, 0x00, 0x00, 0x08, //Win amount
                0x03, //Progressive Level
                0x00, 0x00, 0x00, 0x00, 0x08 //Win amount
            };

            var response = new LongPollAck(true);

            _handler.Setup(c => c.Handle(It.IsAny<MultipleLevelProgressiveBroadcastData>())).Returns(response);
            _target.InjectHandler(_handler.Object);
            var actual = _target.Parse(command).ToList();

            var expectedResults = new List<byte> { TestConstants.SasAddress };

            CollectionAssert.AreEqual(expectedResults, actual);
        }
    }
}