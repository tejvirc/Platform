
namespace Aristocrat.SasClient.Tests.Parsers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Contains the tests for the LPB4SendWagerCategoryInfoParserTest class
    /// </summary>
    [TestClass]
    public class LPB4SendWagerCategoryInfoParserTest
    {
        private const int GameID = 1;
        private const int WagerCat = 6;
        private const int PlaybackPercentage = 9803;
        private const int CoinInMeter = 12444; // in cents
        private const int NumBytesWagerSection = 9;
        private const int FailedPlaybackPercentage = 0;
        private const int FailedCoinInMeter = 0; // in cents
        private const string FailedPercentString = "0000";
        private const int CoinInLength = 9;

        private readonly LPB4SendWagerCategoryInfoParser _target = new LPB4SendWagerCategoryInfoParser(new SasClientConfiguration());
        private readonly Mock<ISasLongPollHandler<LongPollSendWagerResponse, LongPollReadWagerData>> _handler = new Mock<ISasLongPollHandler<LongPollSendWagerResponse, LongPollReadWagerData>>(MockBehavior.Strict);

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendWagerCategoryInformation, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            var responseGood = new LongPollSendWagerResponse(PlaybackPercentage, CoinInMeter, CoinInLength, true);
            var expectedGood = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation
            };
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation
            };

            _handler.Setup(m => m.Handle(It.IsAny<LongPollReadWagerData>())).Returns(responseGood);
            _target.InjectHandler(_handler.Object);

            command.AddRange(Utilities.ToBcd(GameID, SasConstants.Bcd4Digits));
            command.AddRange(Utilities.ToBcd(WagerCat, SasConstants.Bcd4Digits));

            expectedGood.Add((byte)(NumBytesWagerSection + CoinInLength));
            expectedGood.AddRange(Utilities.ToBcd(GameID, SasConstants.Bcd4Digits));
            expectedGood.AddRange(Utilities.ToBcd(WagerCat, SasConstants.Bcd4Digits));
            expectedGood.AddRange(Encoding.ASCII.GetBytes(string.Format("{0:####}", responseGood.PaybackPercentage)));
            expectedGood.Add((byte)CoinInLength);
            expectedGood.AddRange(Utilities.ToBcd((ulong)responseGood.CoinInMeter, CoinInLength));

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expectedGood, actual);
        }

        [TestMethod]
        public void ParseBadWagerCategoryTest()
        {
            var responseBad = new LongPollSendWagerResponse(FailedPlaybackPercentage, FailedCoinInMeter, CoinInLength, false);
            var expectedBad = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation
            };
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation
            };

            command.AddRange(Utilities.ToBcd(GameID, SasConstants.Bcd4Digits));
            command.AddRange(Utilities.ToBcd(WagerCat, SasConstants.Bcd4Digits));

            // now check for a missing wager category. coinInSize should be 0 and no coinInMeter data transmitted
            _handler.Setup(m => m.Handle(It.IsAny<LongPollReadWagerData>())).Returns(responseBad);
            _target.InjectHandler(_handler.Object);

            expectedBad.Add((byte)(NumBytesWagerSection));
            expectedBad.AddRange(Utilities.ToBcd(GameID, SasConstants.Bcd4Digits));
            expectedBad.AddRange(Utilities.ToBcd(WagerCat, SasConstants.Bcd4Digits));
            expectedBad.AddRange(Encoding.ASCII.GetBytes(FailedPercentString));
            expectedBad.Add(0);

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expectedBad, actual);
        }

        [TestMethod]
        public void ParseMissingGameIdTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation
            };
            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(new Collection<byte> { (byte)(command.First() | SasConstants.Nack) }, actual);
        }

        [TestMethod]
        public void ParseMissingWagerCategoryTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendWagerCategoryInformation
            };
            command.AddRange(Utilities.ToBcd(GameID, SasConstants.Bcd4Digits));
            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(new Collection<byte> { (byte)(command.First() | SasConstants.Nack) }, actual);
        }
    }
}
