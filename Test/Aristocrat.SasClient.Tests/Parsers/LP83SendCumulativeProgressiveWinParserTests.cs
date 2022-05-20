﻿namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP83SendCumulativeProgressiveWinParserTests
    {
        private LP83SendCumulativeProgressiveWinParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP83SendCumulativeProgressiveWinParser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendCumulativeProgressiveWins, _target.Command);
        }

        [TestMethod]
        public void ValidParseTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendCumulativeProgressiveWins,
                0x00, 0x00, // game number,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendCumulativeProgressiveWins,
                0x00, 0x00, // game number,
                0x12, 0x34, 0x56, 0x78
            };

            var response = new SendCumulativeProgressiveWinResponse
            {
                MeterValue = 12345678
            };

            var handler =
                new Mock<ISasLongPollHandler<SendCumulativeProgressiveWinResponse, SendCumulativeProgressiveWinData>>(
                    MockBehavior.Default);
            handler.Setup(m => m.Handle(It.IsAny<SendCumulativeProgressiveWinData>())).Returns(response);
            _target.InjectHandler(handler.Object);

            CollectionAssert.AreEqual(expected, _target.Parse(command)?.ToList());
        }

        [TestMethod]
        public void InvalidBcdGameNumberTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendCumulativeProgressiveWins,
                0x00, 0x0A, // invalid BCD in game number,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            var actual = _target.Parse(command).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}