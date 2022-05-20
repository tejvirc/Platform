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
    ///     Contains the tests for the LP9ASendLegacyBonusMetersParser class
    /// </summary>
    [TestClass]
    public class LP9ASendLegacyBonusMetersParserTest
    {
        private LP9ASendLegacyBonusMetersParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP9ASendLegacyBonusMetersParser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendLegacyBonusMeters, _target.Command);
        }

        [TestMethod]
        public void HandleTestSuccess()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendLegacyBonusMeters,
                0x00, 0x01,  // game number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollSendLegacyBonusMetersResponse
            {
                Deductible = 23456789,
                NonDeductible = 34567890,
                WagerMatch = 98765432
            };

            var handler =
                new Mock<ISasLongPollHandler<LongPollSendLegacyBonusMetersResponse, LongPollSendLegacyBonusMetersData>>(
                    MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollSendLegacyBonusMetersData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendLegacyBonusMeters,
                0x00, 0x01,  // game number
                0x23, 0x45, 0x67, 0x89,  // deductible bonus
                0x34, 0x56, 0x78, 0x90,  // non-deductible bonus
                0x98, 0x76, 0x54, 0x32  // wager match bonus
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
                (byte)LongPoll.SendLegacyBonusMeters,
                0x00, 0x0A,  // invalid BCD for game number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var response = new LongPollSendLegacyBonusMetersResponse();

            var handler =
                new Mock<ISasLongPollHandler<LongPollSendLegacyBonusMetersResponse, LongPollSendLegacyBonusMetersData>>(
                    MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<LongPollSendLegacyBonusMetersData>())).Returns(response);
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
