namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     Contains the tests for the LPA4SendCashOutLimitParserTest class
    /// </summary>
    [TestClass]
    public class LPA4SendCashOutLimitParserTest
    {
        private LPA4SendCashOutLimitParser _target = new LPA4SendCashOutLimitParser(new SasClientConfiguration());

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendCashOutLimit, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            var cashoutLimitResponse = new LongPollReadSingleValueResponse<ulong>(123456);
            const ulong GameID = 1;

            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendCashOutLimit
            };
            command.AddRange(Utilities.ToBcd(GameID, SasConstants.Bcd4Digits));

            var handler = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<ulong>, SendCashOutLimitData>>(MockBehavior.Strict);

            handler.Setup(m => m.Handle(It.IsAny<SendCashOutLimitData>())).Returns(cashoutLimitResponse);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendCashOutLimit
            };
            expected.AddRange(Utilities.ToBcd(GameID, SasConstants.Bcd4Digits));
            expected.AddRange(Utilities.ToBcd(cashoutLimitResponse.Data, SasConstants.Bcd4Digits));

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expected, actual);

            // test bad command length
            command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendCashOutLimit
            };

            actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(new Collection<byte> { (byte)(command.First() | SasConstants.Nack) }, actual);
        }
    }
}
