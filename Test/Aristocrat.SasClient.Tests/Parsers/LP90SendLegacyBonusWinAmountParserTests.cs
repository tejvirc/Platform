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
    public class LP90SendLegacyBonusWinAmountParserTests
    {
        private LP90SendLegacyBonusWinAmountParser _target;
        private Mock<ISasLongPollHandler<LegacyBonusWinAmountResponse, LongPollData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP90SendLegacyBonusWinAmountParser(new SasClientConfiguration());

            _handler = new Mock<ISasLongPollHandler<LegacyBonusWinAmountResponse, LongPollData>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendLegacyBonusWinAmount, _target.Command);
        }

        [TestMethod()]
        public void ParseTestWithPropertiesReset()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendLegacyBonusWinAmount
            };

            var response = new LegacyBonusWinAmountResponse()
            {
                MultipliedWin = 0,
                Multiplier = 0,
                BonusAmount = 0,
                TaxStatus = 0
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(response);
            _target.InjectHandler(_handler.Object);
            var actual = _target.Parse(command);

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendLegacyBonusWinAmount,
                0x00, // Multiplier
                0x00, 0x00, 0x00, 0x00, // Multiplier Amount
                0x00, // Tax status
                0x00, 0x00, 0x00, 0x00, // Bonus
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod()]
        public void ParseTestWithPropertiesSet()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendLegacyBonusWinAmount
            };

            var response = new LegacyBonusWinAmountResponse()
            {
                MultipliedWin = 0,
                Multiplier = 0,
                BonusAmount = 12345678,
                TaxStatus = TaxStatus.Nondeductible
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(response);
            _target.InjectHandler(_handler.Object);
            var actual = _target.Parse(command);

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendLegacyBonusWinAmount,
                0x00, // Multiplier
                0x00, 0x00, 0x00, 0x00, // Multiplier Amount
                0x01, // Tax status
                0x12, 0x34, 0x56, 0x78, // Bonus
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }
    }
}