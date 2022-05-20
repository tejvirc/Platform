namespace Aristocrat.SasClient.Tests.Parsers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class LP87SendMultipleSAsProgressiveWinAmountsParserTests
    {
        private Mock<ISasLongPollHandler<SendMultipleSasProgressiveWinAmountsResponse, LongPollData>> _handler;
        private LP87SendMultipleSasProgressiveWinAmountsParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP87SendMultipleSasProgressiveWinAmountsParser();
            _handler = new Mock<ISasLongPollHandler<SendMultipleSasProgressiveWinAmountsResponse, LongPollData>>(
                    MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendMultipleSasProgressiveWinAmounts, _target.Command);
        }

        [TestMethod]
        public void ParseTestWithEmptyProgressiveWinQueue()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendMultipleSasProgressiveWinAmounts
            };

            var response = new SendMultipleSasProgressiveWinAmountsResponse(new List<LinkedProgressiveWinData>(), 1);

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(response);
            _target.InjectHandler(_handler.Object);
            var actual = _target.Parse(command);

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendMultipleSasProgressiveWinAmounts,
                0x02, // Length
                0x01, // Group Id of the Progressive
                0x00 // Number of Progressive Level
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void ParseTestWithNonEmptyProgressiveWinQueue()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendMultipleSasProgressiveWinAmounts
            };

            var winData = new List<LinkedProgressiveWinData>
            {
                new LinkedProgressiveWinData(1, 1234, "TestLevel1"),
                new LinkedProgressiveWinData(2, 5678, "TestLevel2"),
                new LinkedProgressiveWinData(3, 9876, "TestLevel3")
            };

            var response = new SendMultipleSasProgressiveWinAmountsResponse(winData, 1);

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(response);
            _target.InjectHandler(_handler.Object);
            var actual = _target.Parse(command);

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendMultipleSasProgressiveWinAmounts,
                0x14, // Length
                0x01, // Group Id of the Progressive
                0x03, // Number of Progressive Level
                0x01, // level Id 1
                0x00, 0x00, 0x00, 0x12, 0x34, // Amount 1
                0x02, // level Id 2
                0x00, 0x00, 0x00, 0x56, 0x78, // Amount 2
                0x03, // level Id 3
                0x00, 0x00, 0x00, 0x98, 0x76 // Amount 3
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }
    }
}