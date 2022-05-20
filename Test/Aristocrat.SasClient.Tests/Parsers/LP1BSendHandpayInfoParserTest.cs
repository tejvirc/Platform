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
    ///     Contains the tests for the LP1BSendHandpayInfoParserTest class
    /// </summary>
    [TestClass]
    public class LP1BSendHandpayInfoParserTest
    {
        private const byte ClientNumber = 42;
        private LP1BSendHandpayInfoParser _target;
        private Mock<ISasLongPollHandler<LongPollHandpayDataResponse, LongPollHandpayData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP1BSendHandpayInfoParser(new SasClientConfiguration { ClientNumber = ClientNumber });

            _handler = new Mock<ISasLongPollHandler<LongPollHandpayDataResponse, LongPollHandpayData>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendHandpayInformation, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendHandpayInformation
            };

            var response = new LongPollHandpayDataResponse
            {
                ProgressiveGroup = 81,
                Level = LevelId.HandpayCanceledCredits,
                Amount = 1000,
                PartialPayAmount = 0,
                ResetId = 0,
                SessionGameWinAmount = 500,
                SessionGamePayAmount = 0
            };

            _handler.Setup(m => m.Handle(It.Is<LongPollHandpayData>(x => x.ClientNumber == ClientNumber)))
                .Returns(response);
            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendHandpayInformation,
                (byte)response.ProgressiveGroup,
                (byte)response.Level
            };

            expected.AddRange(Utilities.ToBcd((ulong)response.Amount, SasConstants.Bcd10Digits));
            expected.AddRange(Utilities.ToBcd((ulong)response.PartialPayAmount, SasConstants.Bcd4Digits));
            expected.Add((byte)response.ResetId);
            expected.AddRange(Utilities.ToBcd((ulong)response.SessionGameWinAmount, SasConstants.Bcd10Digits));
            expected.AddRange(Utilities.ToBcd((ulong)response.SessionGamePayAmount, SasConstants.Bcd10Digits));

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
