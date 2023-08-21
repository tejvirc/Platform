namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client.LongPollDataClasses;
    using Aristocrat.Sas.Client.LPParsers;
    using Sas.Client;

    [TestClass]
    public class LP71RedeemTicketParserTest
    {
        private Mock<ISasLongPollHandler<RedeemTicketResponse, RedeemTicketData>> _handler;
        private LP71RedeemTicketParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP71RedeemTicketParser();
            _handler = new Mock<ISasLongPollHandler<RedeemTicketResponse, RedeemTicketData>>();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.RedeemTicket, _target.Command);
        }

        [DataRow(RedemptionStatusCode.NoValidationInfoAvailable, DisplayName = "No Validation Information Available returns no data")]
        [DataRow(RedemptionStatusCode.NotCompatibleWithCurrentRedemptionCycle, DisplayName = "Not Compatible With Current Redemption Cycle returns no data")]
        [DataTestMethod]
        public void NoResponseDataTest(RedemptionStatusCode statusCode)
        {
            var response = new RedeemTicketResponse
            {
                MachineStatus = statusCode,
                TransferAmount = 1,
                ParsingCode = ParsingCode.Bcd,
                Barcode = "004054504974162392"
            };

            _handler.Setup(m => m.Handle(It.IsAny<RedeemTicketData>())).Returns(response);
            var expectedTxBytes = new byte[]
            {
                TestConstants.SasAddress, (byte)LongPoll.RedeemTicket,
                0x01, // 1 data bytes in length
                (byte)statusCode, // status code (pending)
            };

            var actualTxBytes = _target.Parse(
                new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.RedeemTicket,
                    0x10, // Length
                    (byte)TicketTransferCode.ValidCashableTicket, // Transfer Code
                    0x00, 0x00, 0x00, 0x00, 0x01, // Amount
                    (byte)ParsingCode.Bcd, // Parsing Code
                    0x00, 0x40, 0x54, 0x50, 0x49, 0x74, 0x16, 0x23, 0x92,  // Bar Code
                    TestConstants.FakeCrc, TestConstants.FakeCrc
                });

            CollectionAssert.AreEquivalent(expectedTxBytes, actualTxBytes.ToList());
        }

        [TestMethod]
        public void NoTargetIdDataIsValid()
        {
            var response = new RedeemTicketResponse
            {
                MachineStatus = RedemptionStatusCode.TicketRedemptionPending,
                TransferAmount = 2500,
                ParsingCode = 0x00,
                Barcode = "001553049347404912"
            };

            _handler.Setup(m => m.Handle(It.IsAny<RedeemTicketData>())).Returns(response);
            var expectedTxBytes = new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RedeemTicket,
                0x10, // 16 data bytes in length
                (byte)RedemptionStatusCode.TicketRedemptionPending, // status code (pending)
                0x00, 0x00, 0x00, 0x25, 0x00, // amount
                (byte)ParsingCode.Bcd, // parsing code
                0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12, // validation data
            };

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RedeemTicket,
                0x16,
                (byte)TicketTransferCode.ValidRestrictedPromotionalTicket,
                0x00, 0x00, 0x00, 0x25, 0x00,
                (byte)ParsingCode.Bcd,
                0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12,
                0x05, 0x30, 0x20, 0x23,
                0x00, 0x01,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actualTxBytes = _target.Parse(command);

            CollectionAssert.AreEquivalent(expectedTxBytes, actualTxBytes.ToList());
        }

        [TestMethod]
        public void ZeroLengthTargetIdDataIsValid()
        {
            var response = new RedeemTicketResponse
            {
                MachineStatus = RedemptionStatusCode.TicketRedemptionPending,
                TransferAmount = 2500,
                ParsingCode = 0x00,
                Barcode = "001553049347404912"
            };

            _handler.Setup(m => m.Handle(It.IsAny<RedeemTicketData>())).Returns(response);
            var expectedTxBytes = new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RedeemTicket,
                0x10, // 16 data bytes in length
                (byte)RedemptionStatusCode.TicketRedemptionPending, // status code (pending)
                0x00, 0x00, 0x00, 0x25, 0x00, // amount
                (byte)ParsingCode.Bcd, // parsing code
                0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12, // validation data
            };

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RedeemTicket,
                0x16,
                (byte)TicketTransferCode.ValidRestrictedPromotionalTicket,
                0x00, 0x00, 0x00, 0x25, 0x00,
                (byte)ParsingCode.Bcd,
                0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12,
                0x05, 0x30, 0x20, 0x23,
                0x00, 0x01,
                0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actualTxBytes = _target.Parse(command);

            CollectionAssert.AreEquivalent(expectedTxBytes, actualTxBytes.ToList());
        }

        [TestMethod]
        public void GetResponseWithNoRedemptionInformation()
        {
            var response = new RedeemTicketResponse
            {
                MachineStatus = RedemptionStatusCode.NoValidationInfoAvailable,
                TransferAmount = 1,
                ParsingCode = ParsingCode.Bcd,
                Barcode = "004054504974162392"
            };

            _handler.Setup(m => m.Handle(It.IsAny<RedeemTicketData>())).Returns(response);
            var expectedTxBytes = new byte[]
            {
                TestConstants.SasAddress, (byte)LongPoll.RedeemTicket,
                0x01, // 1 data bytes in length
                (byte)RedemptionStatusCode.NoValidationInfoAvailable, // status code (pending)
            };

            var actualTxBytes = _target.Parse(
                new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.RedeemTicket,
                    0x10, // Length
                    (byte)TicketTransferCode.ValidCashableTicket, // Transfer Code
                    0x00, 0x00, 0x00, 0x00, 0x01, // Amount
                    (byte)ParsingCode.Bcd, // Parsing Code
                    0x00, 0x40, 0x54, 0x50, 0x49, 0x74, 0x16, 0x23, 0x92,  // Bar Code
                    TestConstants.FakeCrc, TestConstants.FakeCrc
                });

            CollectionAssert.AreEquivalent(expectedTxBytes, actualTxBytes.ToList());
        }

        [TestMethod]
        public void GetRestrictedResponseWithoutRestrictedInfoTest()
        {
            var response = new RedeemTicketResponse
            {
                MachineStatus = RedemptionStatusCode.TicketRedemptionPending,
                TransferAmount = 1,
                ParsingCode = ParsingCode.Bcd,
                Barcode = "004054504974162392"
            };

            _handler.Setup(
                    m => m.Handle(
                        It.Is<RedeemTicketData>(
                            ticketData => ticketData.PoolId == 0 && ticketData.RestrictedExpiration == 0)))
                .Returns(response);
            var expectedTxBytes = new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RedeemTicket,
                0x10, // 16 data bytes in length
                (byte)RedemptionStatusCode.TicketRedemptionPending, // status code (pending)
                0x00, 0x00, 0x00, 0x00, 0x01, // amount
                (byte)ParsingCode.Bcd, // parsing code
                0x00, 0x40, 0x54, 0x50, 0x49, 0x74, 0x16, 0x23, 0x92, // validation data
            };

            var actualTxBytes = _target.Parse(
                new List<byte>
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.RedeemTicket,
                    0x10, // length
                    (byte)TicketTransferCode.ValidRestrictedPromotionalTicket, // Valid restricted promotional ticket
                    0x00, 0x00, 0x00, 0x00, 0x01, // amount 1 cent
                    (byte)ParsingCode.Bcd, // BCD Encoding
                    0x00, 0x40, 0x54, 0x50, 0x49, 0x74, 0x16, 0x23, 0x92, // Bar Code
                    TestConstants.FakeCrc, TestConstants.FakeCrc
                });

            CollectionAssert.AreEquivalent(expectedTxBytes, actualTxBytes.ToList());
        }

        [TestMethod]
        public void GetResponseWithRestrictedInfoTest()
        {
            var response = new RedeemTicketResponse
            {
                MachineStatus = RedemptionStatusCode.TicketRedemptionPending,
                TransferAmount = 2500,
                ParsingCode = 0x00,
                Barcode = "001553049347404912"
            };

            _handler.Setup(m => m.Handle(It.IsAny<RedeemTicketData>())).Returns(response);
            var expectedTxBytes = new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RedeemTicket,
                0x10, // 16 data bytes in length
                (byte)RedemptionStatusCode.TicketRedemptionPending, // status code (pending)
                0x00, 0x00, 0x00, 0x25, 0x00, // amount
                (byte)ParsingCode.Bcd, // parsing code
                0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12, // validation data
            };

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.RedeemTicket,
                0x16,
                (byte)TicketTransferCode.ValidRestrictedPromotionalTicket,
                0x00, 0x00, 0x00, 0x25, 0x00,
                (byte)ParsingCode.Bcd,
                0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12,
                0x05, 0x30, 0x20, 0x23,
                0x00, 0x01,
                0x01,
                0xD2,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var actualTxBytes = _target.Parse(command);

            CollectionAssert.AreEquivalent(expectedTxBytes, actualTxBytes.ToList());
        }

        [DataRow(new byte[]
        {
            TestConstants.SasAddress,
            (byte)LongPoll.RedeemTicket,
            0x16,
            (byte)TicketTransferCode.ValidRestrictedPromotionalTicket,
            0x00, 0x00, 0x00, 0xF5, 0x00,
            (byte)ParsingCode.Bcd,
            0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12,
            0x05, 0x30, 0x20, 0x23,
            0x00, 0x01,
            0x01,
            0xD2,
            TestConstants.FakeCrc, TestConstants.FakeCrc
        }, DisplayName = "Invalid BCD Amount Test")]
        [DataRow(new byte[]
        {
            TestConstants.SasAddress,
            (byte)LongPoll.RedeemTicket,
            0x16,
            (byte)TicketTransferCode.ValidRestrictedPromotionalTicket,
            0x00, 0x00, 0x00, 0x15, 0x00,
            (byte)ParsingCode.Bcd,
            0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12,
            0x05, 0xF0, 0x20, 0x23,
            0x00, 0x01,
            0x01,
            0xD2,
            TestConstants.FakeCrc, TestConstants.FakeCrc
        }, DisplayName = "Invalid BCD Expiration Test")]
        [DataRow(new byte[]
        {
            TestConstants.SasAddress,
            (byte)LongPoll.RedeemTicket,
            0x16,
            (byte)TicketTransferCode.ValidRestrictedPromotionalTicket,
            0x00, 0x00, 0x00, 0x15, 0x00,
            (byte)ParsingCode.Bcd,
            0x00, 0x15, 0x53, 0x04, 0x93, 0x47, 0x40, 0x49, 0x12,
            0x05, 0x30, 0x20, 0x23,
            0x00, 0x01,
            0x02,
            0xD2,
            TestConstants.FakeCrc, TestConstants.FakeCrc
        }, DisplayName = "Invalid Target ID length Test")]
        [DataRow(new byte[]
        {
            TestConstants.SasAddress,
            (byte)LongPoll.RedeemTicket,
            0x0F,
            0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x40, 0x54, 0x50
        }, DisplayName = "Invalid Command Length Test")]
        [DataTestMethod]
        public void InvalidDataTest(byte[] command)
        {
            var expectedTxBytes = new byte[] { TestConstants.SasAddress | SasConstants.Nack };
            var actualTxBytes = _target.Parse(command);
            CollectionAssert.AreEquivalent(expectedTxBytes, actualTxBytes.ToList());
        }
    }
}