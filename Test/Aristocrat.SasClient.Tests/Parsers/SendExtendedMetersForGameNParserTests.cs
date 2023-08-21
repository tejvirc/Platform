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

    [TestClass]
    public class SendExtendedMetersForGameNParserTests
    {
        private const byte ValidGameNumberByte1 = 0x98;
        private const byte ValidGameNumberByte2 = 0x76;
        private const byte InvalidGameNumberByte = 0xAB;

        private static IEnumerable<object[]> ExtendedMetersParsers => new List<object[]>
        {
            new object[]
            {
                new LP6FSendExtendedMetersForGameNParser(new SasClientConfiguration()),
                LongPoll.SendExtendedMetersForGameN
            },
            new object[]
            {
                new LPAFSendExtendedMetersForGameNParser(new SasClientConfiguration()), 
                LongPoll.SendExtendedMetersForGameNAlternate
            }
        };

        private Mock<ISasLongPollHandler<SendSelectedMetersForGameNResponse, LongPollSelectedMetersForGameNData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<SendSelectedMetersForGameNResponse, LongPollSelectedMetersForGameNData>>(MockBehavior.Strict);
        }

        [DynamicData(nameof(ExtendedMetersParsers))]
        [DataTestMethod]
        public void CommandTest(ILongPollParser target, LongPoll longPoll)
        {
            Assert.AreEqual(longPoll, target.Command);
        }

        [DynamicData(nameof(ExtendedMetersParsers))]
        [DataTestMethod]
        public void HandleValidTest(ILongPollParser target, LongPoll longPoll)
        {
            var response = new SendSelectedMetersForGameNResponse(new List<SelectedMeterForGameNResponse>
            {
                new SelectedMeterForGameNResponse(SasMeterId.CurrentCredits, 1234, 0, 8),
                new SelectedMeterForGameNResponse(SasMeterId.GamesPlayed, 5678, 0, 8),
                new SelectedMeterForGameNResponse(SasMeterId.CurrentRestrictedCredits, 9012, 0, 8),
                new SelectedMeterForGameNResponse(SasMeterId.TotalSasCashableTicketInCents, 6789, 0, 10)
            });

            _handler.Setup(c => c.Handle(It.IsAny<LongPollSelectedMetersForGameNData>())).Returns(response);
            target.InjectHandler(_handler.Object);

            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)longPoll,
                0x0A, // number of bytes following
                ValidGameNumberByte1, ValidGameNumberByte2,
                (byte)SasMeterId.CurrentCredits, 0x00,
                (byte)SasMeterId.GamesPlayed, 0x00,
                (byte)SasMeterId.CurrentRestrictedCredits, 0x00,
                (byte)SasMeterId.TotalSasCashableTicketInCents, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)longPoll,
                0x1F, // number of bytes following
                ValidGameNumberByte1, ValidGameNumberByte2,
                (byte)SasMeterId.CurrentCredits, 0x00,
                0x04,
                0x00, 0x00, 0x12, 0x34,
                (byte)SasMeterId.GamesPlayed, 0x00,
                0x04,
                0x00, 0x00, 0x56, 0x78,
                (byte)SasMeterId.CurrentRestrictedCredits, 0x00,
                0x04,
                0x00, 0x00, 0x90, 0x12,
                (byte)SasMeterId.TotalSasCashableTicketInCents, 0x00,
                0x05,
                0x00, 0x00, 0x00, 0x67, 0x89,
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [DynamicData(nameof(ExtendedMetersParsers))]
        [DataTestMethod]
        public void HandleInValidGameByte1Test(ILongPollParser target, LongPoll longPoll)
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)longPoll,
                0x08, // number of bytes following
                InvalidGameNumberByte, ValidGameNumberByte2,
                (byte)SasMeterId.CurrentCredits, 0x00,
                (byte)SasMeterId.GamesPlayed, 0x00,
                (byte)SasMeterId.CurrentRestrictedCredits, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [DynamicData(nameof(ExtendedMetersParsers))]
        [DataTestMethod]
        public void HandleInValidGameByte2Test(ILongPollParser target, LongPoll longPoll)
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)longPoll,
                0x05, // number of bytes following
                ValidGameNumberByte1, InvalidGameNumberByte,
                (byte)SasMeterId.CurrentCredits,
                (byte)SasMeterId.GamesPlayed,
                (byte)SasMeterId.CurrentRestrictedCredits,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [DynamicData(nameof(ExtendedMetersParsers))]
        [DataTestMethod]
        public void HandleInvalidLengthTooSmallTest(ILongPollParser target, LongPoll longPoll)
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)longPoll,
                0x01, // number of bytes following
                ValidGameNumberByte1, ValidGameNumberByte2,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actual = target.Parse(command).ToList();

            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }

        [DynamicData(nameof(ExtendedMetersParsers))]
        [DataTestMethod]
        public void HandleLengthTruncationTest(ILongPollParser target, LongPoll longPoll)
        {
            var response = new SendSelectedMetersForGameNResponse(new List<SelectedMeterForGameNResponse>
            {
                new SelectedMeterForGameNResponse(SasMeterId.CurrentCredits, 1234, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.CurrentRestrictedCredits, 9012, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.GamesPlayed, 5678, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.GamesWon, 5236, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.GamesLost, 8752, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalBillsInStacker, 1571, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalAttendantPaidExternalBonus, 789, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalAttendantPaidProgressiveWin, 20000, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalCanceledCredits, 1000, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalCashableTicketIn, 9000, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalCoinIn, 200000, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.AftCashableIn, 100000, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.AftCashableBonusInQuantity, 100, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalWonCredits, 3000000, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalTicketOut, 65000, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.RestrictedTicketInCents, 321, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.RestrictedTicketOutCents, 200, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.TotalBillAmountInStacker, 854710, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.NonRestrictedTicketInCents, 65847103, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.NonRestrictedTicketInCount, 985123, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.RestrictedTicketInCount, 558413, 4, SasConstants.MaxMeterLength),
                new SelectedMeterForGameNResponse(SasMeterId.RestrictedTicketOutCount, 985417, 4, SasConstants.MaxMeterLength)
            });

            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)longPoll,
                0x2E, // number of bytes following
                ValidGameNumberByte1, ValidGameNumberByte2,
                (byte)SasMeterId.CurrentCredits, 0x00,
                (byte)SasMeterId.CurrentRestrictedCredits, 0x00,
                (byte)SasMeterId.GamesPlayed, 0x00,
                (byte)SasMeterId.GamesWon, 0x00,
                (byte)SasMeterId.GamesLost, 0x00,
                (byte)SasMeterId.TotalBillsInStacker, 0x00,
                (byte)SasMeterId.TotalAttendantPaidExternalBonus, 0x00,
                (byte)SasMeterId.TotalAttendantPaidProgressiveWin, 0x00,
                (byte)SasMeterId.TotalCanceledCredits, 0x00,
                (byte)SasMeterId.TotalCashableTicketIn, 0x00,
                (byte)SasMeterId.TotalCoinIn, 0x00,
                (byte)SasMeterId.AftCashableIn, 0x00,
                (byte)SasMeterId.AftCashableBonusInQuantity, 0x00,
                (byte)SasMeterId.TotalWonCredits, 0x00,
                (byte)SasMeterId.TotalTicketOut, 0x00,
                (byte)SasMeterId.RestrictedTicketInCents, 0x00,
                (byte)SasMeterId.RestrictedTicketOutCents, 0x00,
                (byte)SasMeterId.TotalBillAmountInStacker, 0x00,
                (byte)SasMeterId.NonRestrictedTicketInCents, 0x00,
                (byte)SasMeterId.NonRestrictedTicketInCount, 0x00,
                (byte)SasMeterId.RestrictedTicketInCount, 0x00,
                (byte)SasMeterId.RestrictedTicketOutCount, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            _handler.Setup(c => c.Handle(It.IsAny<LongPollSelectedMetersForGameNData>())).Returns(response);
            target.InjectHandler(_handler.Object);

            var actual = target.Parse(command).ToList();
            const int expectedLength = SasConstants.MaxMeterLength / 2;

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)longPoll,
                0xFE, // number of bytes following
                ValidGameNumberByte1, ValidGameNumberByte2,
                (byte)SasMeterId.CurrentCredits, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x12, 0x34,
                (byte)SasMeterId.CurrentRestrictedCredits, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x12,
                (byte)SasMeterId.GamesPlayed, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x56, 0x78,
                (byte)SasMeterId.GamesWon, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x52, 0x36,
                (byte)SasMeterId.GamesLost, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x87, 0x52,
                (byte)SasMeterId.TotalBillsInStacker, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x15, 0x71,
                (byte)SasMeterId.TotalAttendantPaidExternalBonus, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x89,
                (byte)SasMeterId.TotalAttendantPaidProgressiveWin, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00,
                (byte)SasMeterId.TotalCanceledCredits, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00,
                (byte)SasMeterId.TotalCashableTicketIn, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x00,
                (byte)SasMeterId.TotalCoinIn, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00,
                (byte)SasMeterId.AftCashableIn, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00,
                (byte)SasMeterId.AftCashableBonusInQuantity, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
                (byte)SasMeterId.TotalWonCredits, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
                (byte)SasMeterId.TotalTicketOut, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x50, 0x00,
                (byte)SasMeterId.RestrictedTicketInCents, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x21,
                (byte)SasMeterId.RestrictedTicketOutCents, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00,
                (byte)SasMeterId.TotalBillAmountInStacker, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x85, 0x47, 0x10,
                (byte)SasMeterId.NonRestrictedTicketInCents, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x65, 0x84, 0x71, 0x03,
                (byte)SasMeterId.NonRestrictedTicketInCount, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x98, 0x51, 0x23,
                (byte)SasMeterId.RestrictedTicketInCount, 0x00,
                expectedLength,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x55, 0x84, 0x13,
            };

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}