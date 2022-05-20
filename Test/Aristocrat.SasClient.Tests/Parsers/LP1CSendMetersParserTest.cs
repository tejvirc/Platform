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
    ///     Contains the tests for the LP1CSendMetersParser class
    /// </summary>
    [TestClass]
    public class LP1CSendMetersParserTest
    {
        private LP1CSendMetersParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP1CSendMetersParser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendMeters, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.SendMeters };

            var response = new LongPollReadMultipleMetersResponse();
            response.Meters.Add(SasMeters.TotalCoinIn, new LongPollReadMeterResponse(SasMeters.TotalCoinIn, 12345678));
            response.Meters.Add(SasMeters.TotalCoinOut, new LongPollReadMeterResponse(SasMeters.TotalCoinOut, 23456789));
            response.Meters.Add(SasMeters.TotalDrop, new LongPollReadMeterResponse(SasMeters.TotalDrop, 34567890));
            response.Meters.Add(SasMeters.TotalJackpot, new LongPollReadMeterResponse(SasMeters.TotalJackpot, 45678901));
            response.Meters.Add(SasMeters.GamesPlayed, new LongPollReadMeterResponse(SasMeters.GamesPlayed, 56789012));
            response.Meters.Add(SasMeters.GamesWon, new LongPollReadMeterResponse(SasMeters.GamesWon, 6789012));
            response.Meters.Add(SasMeters.MainDoorOpened, new LongPollReadMeterResponse(SasMeters.MainDoorOpened, 7890123));
            response.Meters.Add(SasMeters.TopMainDoorOpened, new LongPollReadMeterResponse(SasMeters.TopMainDoorOpened, 8901234));
            response.Meters.Add(SasMeters.PowerReset, new LongPollReadMeterResponse(SasMeters.PowerReset, 78901234));

            var handler = new Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>>(MockBehavior.Strict);
            handler.Setup(m => m.Handle(It.IsAny<LongPollReadMultipleMetersData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendMeters,
                0x12, 0x34, 0x56, 0x78,  // Total coin in
                0x23, 0x45, 0x67, 0x89,  // Total coin out
                0x34, 0x56, 0x78, 0x90,  // Total drop
                0x45, 0x67, 0x89, 0x01,  // Total jackpot
                0x56, 0x78, 0x90, 0x12,  // Games played
                0x06, 0x78, 0x90, 0x12,  // Games won    
                0x16, 0x79, 0x13, 0x57,  // Door opened            
                0x78, 0x90, 0x12, 0x34   // Power reset                 
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
