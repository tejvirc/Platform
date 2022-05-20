namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP4FSendCurrentHopperStatusParserTest
    {
        private LP4FSendCurrentHopperStatusParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var response = new LongPollHopperStatusResponse
            {
                Level = 0,
                PercentFull = 0xFF,
                Status = LongPollHopperStatusResponse.HopperStatus.HopperEmpty
            };

            var handler =
                new Mock<ISasLongPollHandler<LongPollHopperStatusResponse, LongPollData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(response);

            _target = new LP4FSendCurrentHopperStatusParser();
            _target.InjectHandler(handler.Object);
        }

        [TestMethod]
        public void ParseTest()
        {
            var longPoll = new Collection<byte> { TestConstants.SasAddress, 0x4F };
            var expected = new byte[]
            {
                TestConstants.SasAddress, 0x4F,
                0x06, // Length
                0x07, // Status
                0xFF, // % Full
                0, 0, 0, 0 // Level
            };

            CollectionAssert.AreEqual(expected, _target.Parse(longPoll).ToList());
        }
    }
}