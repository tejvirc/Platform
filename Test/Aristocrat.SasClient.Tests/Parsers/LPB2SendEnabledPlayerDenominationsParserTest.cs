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
    ///     Contains the tests for the LPB2SendEnabledPlayerDenominationsParserTest class
    /// </summary>
    [TestClass]
    public class LPB2SendEnabledPlayerDenominationsParserTest
    {
        private readonly LPB2SendEnabledPlayerDenominationsParser _target = new LPB2SendEnabledPlayerDenominationsParser();
        private readonly Mock<ISasLongPollHandler<LongPollEnabledPlayerDenominationsResponse, LongPollData>> _handler =
            new Mock<ISasLongPollHandler<LongPollEnabledPlayerDenominationsResponse, LongPollData>>(MockBehavior.Strict);

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendEnabledPlayerDenominations, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            var expectedGoodDenominationsCodes = new byte[] { 1, 10, 5, 20 };
            var responseGood = new LongPollEnabledPlayerDenominationsResponse(expectedGoodDenominationsCodes);
            var expectedGood = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendEnabledPlayerDenominations,
                (byte)(expectedGoodDenominationsCodes.Length + 1),
                (byte)expectedGoodDenominationsCodes.Length
            };
            expectedGood.AddRange(expectedGoodDenominationsCodes);
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendEnabledPlayerDenominations
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
            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns((LongPollEnabledPlayerDenominationsResponse)null);
            _target.InjectHandler(_handler.Object);

            var actual = _target.Parse(command);
            Assert.IsNull(actual);
        }
    }
}
