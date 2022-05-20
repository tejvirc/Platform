namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP55SendSelectedGameNumberParserTest
    {
        private LP55SendSelectedGameNumberParser _target;
        private Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<int>, LongPollData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP55SendSelectedGameNumberParser();
            _handler =
                new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<int>, LongPollData>>(MockBehavior.Default);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendSelectedGameNumber, _target.Command);
        }

        [TestMethod]
        public void ParserTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendSelectedGameNumber
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>()))
                .Returns(new LongPollReadSingleValueResponse<int>(10));

            var result = _target.Parse(command);
            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendSelectedGameNumber,
                0x00, 0x10  // Game ID BCD
            };

            CollectionAssert.AreEqual(expected, result.ToList());
        }
    }
}