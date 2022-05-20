namespace Aristocrat.SasClient.Tests.Parsers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Contains the tests for the LPB1SendCurrentPlayerDenomParserTest class
    /// </summary>
    [TestClass]
    public class LPB1SendCurrentPlayerDenomParserTest
    {
        private readonly LPB1SendCurrentPlayerDenomParser _target = new LPB1SendCurrentPlayerDenomParser();
        private readonly Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LongPollData>> _handler =
            new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LongPollData>>(MockBehavior.Strict);


        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendCurrentPlayerDenominations, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            const byte ExpectedGoodCode = 1;

            var responseGood = new LongPollReadSingleValueResponse<byte>(ExpectedGoodCode);
            var expectedGood = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation, ExpectedGoodCode
            };
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation
            };

            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(responseGood);
            _target.InjectHandler(_handler.Object);

            var actual = _target.Parse(command).ToList();
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(expectedGood, actual);
        }

        [TestMethod]
        public void ParseFailTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation
            };

            // basically if the handler returned null because info was not found the the parser returns null
            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns((LongPollReadSingleValueResponse<byte>)null);
            _target.InjectHandler(_handler.Object);

            var actual = _target.Parse(command);
            Assert.IsNull(actual);
        }
    }
}

