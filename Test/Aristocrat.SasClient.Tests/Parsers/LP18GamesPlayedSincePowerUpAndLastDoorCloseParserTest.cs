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
    ///     Contains the tests for the LP18SendGamesPlayedSinceLastPowerUpAndDoorCloseParser class
    /// </summary>
    [TestClass]
    public class LP18SendGamesPlayedSinceLastPowerUpAndDoorCloseParserTest
    {
        private LP18SendGamesPlayedSinceLastPowerUpAndDoorCloseParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP18SendGamesPlayedSinceLastPowerUpAndDoorCloseParser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendGamesSincePowerUpLastDoorMeter, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.SendGamesSincePowerUpLastDoorMeter };

            var response = new LongPollReadMultipleMetersResponse();
            response.Meters.Add(SasMeters.GamesSinceLastPowerUp, new LongPollReadMeterResponse(SasMeters.GamesSinceLastPowerUp, 1234));
            response.Meters.Add(SasMeters.GamesSinceLastDoorClose, new LongPollReadMeterResponse(SasMeters.GamesSinceLastDoorClose, 2345));

            var handler = new Mock<ISasLongPollHandler<LongPollReadMultipleMetersResponse, LongPollReadMultipleMetersData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollReadMultipleMetersData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendGamesSincePowerUpLastDoorMeter,
                0x12, 0x34,  // games played since power up
                0x23, 0x45   // games played since door close
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
