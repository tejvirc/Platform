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
    public class LPB3SendTokenDenominationParserTest
    {
        private LPB3SendTokenDenominationParser _target;
        private Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LongPollData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LPB3SendTokenDenominationParser();

            _handler = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LongPollData>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendTokenDenomination, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            const byte tokenDenomCode = 0x18;
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendTokenDenomination
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(new LongPollReadSingleValueResponse<byte>(tokenDenomCode));

            var actual = _target.Parse(command).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendTokenDenomination,
                tokenDenomCode
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }
    }
}