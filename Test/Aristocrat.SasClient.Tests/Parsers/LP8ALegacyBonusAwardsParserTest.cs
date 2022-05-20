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
    ///     Contains the tests for the LP8ALegacyBonusAwardsParser class
    /// </summary>
    [TestClass]
    public class LP8ALegacyBonusAwardsParserTest
    {
        private const byte RespondNack = 0;
        private const byte RespondIgnore = 0;
        private const byte RespondAck = 1;
        private const byte RespondBusy = 2;
        private LP8ALegacyBonusAwardsParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP8ALegacyBonusAwardsParser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.InitiateLegacyBonusPay, _target.Command);
        }

        [TestMethod]
        public void HandleTestSuccess()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.InitiateLegacyBonusPay,
                0x00, 0x00, 0x99, 0x99,  // Bonus amount in SAS accounting denom
                0x01,  // Tax status
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollReadSingleValueResponse<byte>(RespondAck);

            Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>> handler
                = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LegacyBonusAwardsData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleTestFailure()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.InitiateLegacyBonusPay,
                0x00, 0x00, 0x99, 0x99,  // Bonus amount in SAS accounting denom
                0x01,  // Tax status
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollReadSingleValueResponse<byte>(RespondIgnore);

            Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>> handler
                = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LegacyBonusAwardsData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var actual = _target.Parse(command)?.ToList();

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void HandleBadBonusAmountBcdTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNMeters,
                0x00, 0x00, 0x99, 0x9A,  // Bonus amount in SAS accounting denom
                0x01,  // Tax status
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollReadSingleValueResponse<byte>(RespondNack);

            Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>> handler
                = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LegacyBonusAwardsData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleInvalidTaxStatusTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNMeters,
                0x00, 0x00, 0x99, 0x99,  // Bonus amount in SAS accounting denom
                0x04,  // Tax status
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollReadSingleValueResponse<byte>(RespondNack);

            Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>> handler
                = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LegacyBonusAwardsData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleSystemDisableTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNMeters,
                0x00, 0x00, 0x99, 0x99,  // Bonus amount in SAS accounting denom
                0x01,  // Tax status
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollReadSingleValueResponse<byte>(RespondBusy);

            Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>> handler
                = new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<byte>, LegacyBonusAwardsData>>(MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LegacyBonusAwardsData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress , TestConstants.BusyResponse
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
