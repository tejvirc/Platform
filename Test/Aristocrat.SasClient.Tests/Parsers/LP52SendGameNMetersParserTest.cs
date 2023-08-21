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
    ///     Contains the tests for the LP52SendGameNMetersParser class
    /// </summary>
    [TestClass]
    public class LP52SendGameNMetersParserTest
    {
        private LP52SendGameNMetersParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP52SendGameNMetersParser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendGameNMeters, _target.Command);
        }

        [TestMethod]
        public void HandleTestSuccess()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNMeters,
                0x00, 0x01,  // game number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollReadMultipleMetersResponse();
            response.Meters.Add(SasMeters.TotalCoinIn, new LongPollReadMeterResponse(SasMeters.TotalCoinIn, 23456789));
            response.Meters.Add(SasMeters.TotalCoinOut, new LongPollReadMeterResponse(SasMeters.TotalCoinOut, 34567890));
            response.Meters.Add(SasMeters.TotalJackpot, new LongPollReadMeterResponse(SasMeters.TotalJackpot, 98765432));
            response.Meters.Add(SasMeters.GamesPlayed, new LongPollReadMeterResponse(SasMeters.GamesPlayed, 00004321));


            Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersGameNData >> handler
                = new Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersGameNData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollReadMultipleMetersGameNData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNMeters,
                0x00, 0x01,  // game number
                0x23, 0x45, 0x67, 0x89,  // total in
                0x34, 0x56, 0x78, 0x90,  // total out
                0x98, 0x76, 0x54, 0x32,  // jackpot
                0x00, 0x00, 0x43, 0x21   // games played
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleBadGameNumberBcdTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNMeters,
                0x00, 0x0A,  // invalid BCD for game number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollReadMultipleMetersResponse();

            Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersGameNData>> handler
                = new Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersGameNData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollReadMultipleMetersGameNData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
