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
    ///     Contains the tests for the LP0FSendMeters10Through15Parser class
    /// </summary>
    [TestClass]
    public class LP0FSendMeters10Through15ParserTest
    {
        private LP0FSendMeters10Through15Parser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP0FSendMeters10Through15Parser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendMeters10Thru15, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.SendMeters10Thru15 };

            var response = new LongPollReadMultipleMetersResponse();
            response.Meters.Add(SasMeters.TotalCanceledCredits, new LongPollReadMeterResponse(SasMeters.TotalCanceledCredits, 12345678));
            response.Meters.Add(SasMeters.TotalCoinIn, new LongPollReadMeterResponse(SasMeters.TotalCoinIn, 23456789));
            response.Meters.Add(SasMeters.TotalCoinOut, new LongPollReadMeterResponse(SasMeters.TotalCoinOut, 34567890));
            response.Meters.Add(SasMeters.TotalDrop, new LongPollReadMeterResponse(SasMeters.TotalDrop, 00001234));
            response.Meters.Add(SasMeters.TotalJackpot, new LongPollReadMeterResponse(SasMeters.TotalJackpot, 98765432));
            response.Meters.Add(SasMeters.GamesPlayed, new LongPollReadMeterResponse(SasMeters.GamesPlayed, 00004321));

            Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>> handler
                = new Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollReadMultipleMetersData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendMeters10Thru15,
                0x12, 0x34, 0x56, 0x78,  // canceled credits
                0x23, 0x45, 0x67, 0x89,  // total in
                0x34, 0x56, 0x78, 0x90,  // total out
                0x00, 0x00, 0x12, 0x34,  // total cash in
                0x98, 0x76, 0x54, 0x32,  // jackpot
                0x00, 0x00, 0x43, 0x21   // games played
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
