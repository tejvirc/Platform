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
    ///     Contains the unit tests for the LP8ESendCardInformationParser class
    /// </summary>
    [TestClass]
    public class LP8ESendCardInformationParserTest
    {
        private LP8ESendCardInformationParser _target;
        private readonly SendCardInformationResponse _response = new SendCardInformationResponse
        {
            FinalHand = true,
            Card1 = SasCard.Clubs10,
            Card2 = SasCard.Diamonds3,
            Card3 = SasCard.Joker,
            Card4 = SasCard.Spades8,
            Card5 = SasCard.HeartsAce
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP8ESendCardInformationParser();
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendCardInformation, _target.Command);
        }

        [TestMethod]
        public void ParseTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendCardInformation
            };

            var handler = new Mock<ISasLongPollHandler<SendCardInformationResponse, LongPollData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(_response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendCardInformation,
                0x01, // final hand
                24,   // 10 of clubs
                49,   // 3 of diamonds
                0x4D, // joker
                6,    // 8 of spades
                44    // ace of hearts
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
