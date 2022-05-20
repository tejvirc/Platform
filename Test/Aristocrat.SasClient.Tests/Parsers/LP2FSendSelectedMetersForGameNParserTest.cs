namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;
    using Sas.Client.Metering;

    /// <summary>
    ///     Contains the tests for the LP2FSendSelectedMetersForGameNParser class
    /// </summary>
    [TestClass]
    public class LP2FSendSelectedMetersForGameNParserTest
    {
        private const byte ValidGameNumberByte1 = 0x98;
        private const byte ValidGameNumberByte2 = 0x76;
        private const byte InvalidGameNumberByte = 0xAB;

        private Mock<ISasLongPollHandler<SendSelectedMetersForGameNResponse, LongPollSelectedMetersForGameNData>> _handler;
        private LP2FSendSelectedMetersForGameNParser _target;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<SendSelectedMetersForGameNResponse, LongPollSelectedMetersForGameNData>>(MockBehavior.Strict);
            _target = new LP2FSendSelectedMetersForGameNParser(new SasClientConfiguration());
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendSelectedMetersForGameN, _target.Command);
        }

        [TestMethod]
        public void HandleValidTest()
        {
            var response = new SendSelectedMetersForGameNResponse(new List<SelectedMeterForGameNResponse>
            {
                new SelectedMeterForGameNResponse(SasMeterId.CurrentCredits, 1234, 4, 4),
                new SelectedMeterForGameNResponse(SasMeterId.GamesPlayed, 5678, 4, 4),
                new SelectedMeterForGameNResponse(SasMeterId.CurrentRestrictedCredits, 9012, 4, 4),
                new SelectedMeterForGameNResponse(SasMeterId.TotalSasCashableTicketInCents, 6789, 5, 5)
            });

            _handler.Setup(c => c.Handle(It.IsAny<LongPollSelectedMetersForGameNData>())).Returns(response);
            _target.InjectHandler(_handler.Object);

            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x06, // number of bytes following
                ValidGameNumberByte1, ValidGameNumberByte2,
                (byte)SasMeterId.CurrentCredits,
                (byte)SasMeterId.GamesPlayed,
                (byte)SasMeterId.CurrentRestrictedCredits,
                (byte)SasMeterId.TotalSasCashableTicketInCents,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x17, // number of bytes following
                ValidGameNumberByte1, ValidGameNumberByte2,
                (byte)SasMeterId.CurrentCredits,
                0x00, 0x00, 0x12, 0x34,
                (byte)SasMeterId.GamesPlayed,
                0x00, 0x00, 0x56, 0x78,
                (byte)SasMeterId.CurrentRestrictedCredits,
                0x00, 0x00, 0x90, 0x12,
                (byte)SasMeterId.TotalSasCashableTicketInCents,
                0x00, 0x00, 0x00, 0x67, 0x89,
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleInValidGameByte1Test()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x05, // number of bytes following
                InvalidGameNumberByte, ValidGameNumberByte2,
                (byte)SasMeterId.CurrentCredits,
                (byte)SasMeterId.GamesPlayed,
                (byte)SasMeterId.CurrentRestrictedCredits,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleInValidGameByte2Test()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x05, // number of bytes following
                ValidGameNumberByte1, InvalidGameNumberByte,
                (byte)SasMeterId.CurrentCredits,
                (byte)SasMeterId.GamesPlayed,
                (byte)SasMeterId.CurrentRestrictedCredits,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleInvalidLengthTooSmallTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x02, // number of bytes following
                ValidGameNumberByte1, InvalidGameNumberByte,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleInvalidLengthTooLargeTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x0D, // number of bytes following
                ValidGameNumberByte1, InvalidGameNumberByte,
                (byte)SasMeterId.CurrentCredits,
                (byte)SasMeterId.CurrentRestrictedCredits,
                (byte)SasMeterId.GamesPlayed,
                (byte)SasMeterId.GamesWon,
                (byte)SasMeterId.GamesLost,
                (byte)SasMeterId.TotalBillsInStacker,
                (byte)SasMeterId.TotalAttendantPaidExternalBonus,
                (byte)SasMeterId.TotalAttendantPaidProgressiveWin,
                (byte)SasMeterId.TotalCanceledCredits,
                (byte)SasMeterId.TotalCashableTicketIn,
                (byte)SasMeterId.TotalCoinIn,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleValidDenomAwareTest()
        {
            var response = new SendSelectedMetersForGameNResponse(new List<SelectedMeterForGameNResponse>
            {
                new SelectedMeterForGameNResponse(SasMeterId.TotalCoinIn, 1234, 4, 4),
                new SelectedMeterForGameNResponse(SasMeterId.TotalCoinOut, 1200, 4, 4)
            });

            _handler.Setup(c => c.Handle(It.IsAny<LongPollSelectedMetersForGameNData>())).Returns(response);
            _target.InjectHandler(_handler.Object);

            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x04, // length
                ValidGameNumberByte1, ValidGameNumberByte2,
                (byte)SasMeterId.TotalCoinIn,
                (byte)SasMeterId.TotalCoinOut,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x0C, //length
                ValidGameNumberByte1, ValidGameNumberByte2,
                (byte)SasMeterId.TotalCoinIn,
                0x00, 0x00, 0x12, 0x34,
                (byte)SasMeterId.TotalCoinOut,
                0x00, 0x00, 0x12, 0x00,
            };

            var actual = _target.Parse(command, 1).ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleInvalidDenomAwareTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSelectedMetersForGameN,
                0x0D, // number of bytes following
                ValidGameNumberByte1, InvalidGameNumberByte,
                (byte)SasMeterId.CurrentCredits,
                (byte)SasMeterId.CurrentRestrictedCredits,
                (byte)SasMeterId.GamesPlayed,
                (byte)SasMeterId.GamesWon,
                (byte)SasMeterId.GamesLost,
                (byte)SasMeterId.TotalBillsInStacker,
                (byte)SasMeterId.TotalAttendantPaidExternalBonus,
                (byte)SasMeterId.TotalAttendantPaidProgressiveWin,
                (byte)SasMeterId.TotalCanceledCredits,
                (byte)SasMeterId.TotalCashableTicketIn,
                (byte)SasMeterId.TotalCoinIn,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = _target.Parse(command, 1).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress, 0x00, (byte)MultiDenomAwareErrorCode.ImproperlyFormatted
            };

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
