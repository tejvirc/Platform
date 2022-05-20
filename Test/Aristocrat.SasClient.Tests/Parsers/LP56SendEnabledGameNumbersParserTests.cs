namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP56SendEnabledGameNumbersParserTests
    {
        private LP56SendEnabledGameNumbersParser _target;
        private Mock<ISasLongPollMultiDenomAwareHandler<SendEnabledGameNumbersResponse, LongPollMultiDenomAwareData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP56SendEnabledGameNumbersParser();

            _handler = new Mock<ISasLongPollMultiDenomAwareHandler<SendEnabledGameNumbersResponse, LongPollMultiDenomAwareData>>(MockBehavior.Default);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendEnabledGameNumbers, _target.Command);
        }

        [TestMethod]
        public void HandleTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnabledGameNumbers,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollMultiDenomAwareData>()))
                .Returns(new SendEnabledGameNumbersResponse(new List<long> { 2, 10 }));

            var actual = _target.Parse(command).ToArray();

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnabledGameNumbers,
                0x05, // Length
                0x02, // Number of games
                0x00, 0x02, // Game number BCD
                0x00, 0x10 // Game number BCD
            };

            CollectionAssert.AreEqual(expectedResults, actual.ToList());
        }

        [TestMethod]
        public void HandleDenomAwareOneCentTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnabledGameNumbers,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            _handler.Setup(c => c.Handle(It.Is<LongPollMultiDenomAwareData>(e => e.MultiDenomPoll && e.TargetDenomination == 1)))
                .Returns(new SendEnabledGameNumbersResponse(new List<long> { 1, 3 }));
            _handler.Setup(c => c.Handle(It.Is<LongPollMultiDenomAwareData>(e => e.MultiDenomPoll && e.TargetDenomination == 5)))
                .Returns(new SendEnabledGameNumbersResponse(new List<long> { 1 }));

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnabledGameNumbers,
                0x05, // Length
                0x02, // Number of games
                0x00, 0x01, // Game number BCD
                0x00, 0x03 // Game number BCD
            };

            var actualResults = _target.Parse(command, 1);

            CollectionAssert.AreEqual(expectedResults, actualResults.ToList());
        }

        [TestMethod]
        public void HandleDenomAwareFiveCentTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnabledGameNumbers
            };

            _handler.Setup(c => c.Handle(It.Is<LongPollMultiDenomAwareData>(e => e.MultiDenomPoll && e.TargetDenomination == 1)))
                .Returns(new SendEnabledGameNumbersResponse(new List<long> { 1, 3 }));
            _handler.Setup(c => c.Handle(It.Is<LongPollMultiDenomAwareData>(e => e.MultiDenomPoll && e.TargetDenomination == 5)))
                .Returns(new SendEnabledGameNumbersResponse(new List<long> { 1 }));

            var expectedResults = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendEnabledGameNumbers,
                0x03, // Length
                0x01, // Number of games
                0x00, 0x01, // Game number BCD
            };

            var actualResults = _target.Parse(command, 5);

            CollectionAssert.AreEqual(expectedResults, actualResults.ToList());
        }
    }
}