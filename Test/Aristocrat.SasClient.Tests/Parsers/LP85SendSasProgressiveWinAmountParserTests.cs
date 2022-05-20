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
    public class LP85SendSasProgressiveWinAmountParserTests
    {
        private Mock<ISasLongPollHandler<SendProgressiveWinAmountResponse, LongPollData>> _handler;

        private LP85SendSasProgressiveWinAmountParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP85SendSasProgressiveWinAmountParser();

            _handler =
                new Mock<ISasLongPollHandler<SendProgressiveWinAmountResponse, LongPollData>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendSasProgressiveWinAmount, _target.Command);
        }

        [TestMethod]
        public void ParseTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.SendSasProgressiveWinAmount };
            var response = new SendProgressiveWinAmountResponse { LevelId = 4, GroupId = 5, WinAmount = 8 };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollData>())).Returns(response);
            _target.InjectHandler(_handler.Object);
            var actual = _target.Parse(command);

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendSasProgressiveWinAmount,
                0x05, //Group Id of the Progressive
                0x04, //Progressive Level
                0x00, 0x00, 0x00, 0x00, 0x08 //Win amount
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }
    }
}