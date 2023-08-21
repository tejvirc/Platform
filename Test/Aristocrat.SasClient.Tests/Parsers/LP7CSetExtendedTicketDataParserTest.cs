namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP7CSetExtendedTicketDataParserTest
    {
        private LP7CSetExtendedTicketDataParser _target;
        private Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<TicketDataStatus>, SetTicketData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler =
                new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<TicketDataStatus>, SetTicketData>>(
                    MockBehavior.Default);
            _target = new LP7CSetExtendedTicketDataParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SetExtendedTicketData, _target.Command);
        }

        [DataRow(
            "location",
            "address1",
            "address2",
            "restricted",
            "debit",
            TicketDataStatus.ValidData,
            DisplayName = "All values can be sent")]
        [DataRow(
            "1234567890123456789012345678901234567890",
            null,
            null,
            null,
            null,
            TicketDataStatus.ValidData,
            DisplayName = "Only Location is valid")]
        [DataRow(
            null,
            "1234567890123456789012345678901234567890",
            null,
            null,
            null,
            TicketDataStatus.ValidData,
            DisplayName = "Only Address1 is valid")]
        [DataRow(
            null,
            null,
            "1234567890123456789012345678901234567890",
            null,
            null,
            TicketDataStatus.ValidData,
            DisplayName = "Only Address2 is valid")]
        [DataRow(
            null,
            null,
            null,
            "1234567890123456",
            null,
            TicketDataStatus.ValidData,
            DisplayName = "Only restricted ticket title is valid")]
        [DataRow(
            null,
            null,
            null,
            null,
            "1234567890123456",
            TicketDataStatus.ValidData,
            DisplayName = "Only debit ticket title is valid")]
        [DataRow(
            "",
            "",
            "",
            "",
            "",
            TicketDataStatus.InvalidData,
            DisplayName = "Empty Values are valid")]
        [DataTestMethod]
        public void ValidParsingCommandTest(
            string location,
            string address1,
            string address2,
            string restrictedTicketTitle,
            string debitTicketTitle,
            TicketDataStatus response)
        {
            var expectedResponse = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                (byte) response
            };

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                0 // Will build later
            };

            if (location != null)
            {
                command.Add((byte) ExtendTicketDataCode.Location);
                command.Add((byte)location.Length);
                command.AddRange(Encoding.ASCII.GetBytes(location));
            }

            if (address1 != null)
            {
                command.Add((byte)ExtendTicketDataCode.Address1);
                command.Add((byte)address1.Length);
                command.AddRange(Encoding.ASCII.GetBytes(address1));
            }

            if (address2 != null)
            {
                command.Add((byte)ExtendTicketDataCode.Address2);
                command.Add((byte)address2.Length);
                command.AddRange(Encoding.ASCII.GetBytes(address2));
            }

            if (restrictedTicketTitle != null)
            {
                command.Add((byte)ExtendTicketDataCode.RestrictedTicketTitle);
                command.Add((byte)restrictedTicketTitle.Length);
                command.AddRange(Encoding.ASCII.GetBytes(restrictedTicketTitle));
            }

            if (debitTicketTitle != null)
            {
                command.Add((byte)ExtendTicketDataCode.DebitTicketTitle);
                command.Add((byte)debitTicketTitle.Length);
                command.AddRange(Encoding.ASCII.GetBytes(debitTicketTitle));
            }

            command[SasConstants.SasLengthIndex] = (byte)(command.Count - (SasConstants.SasLengthIndex + 1));
            command.Add(TestConstants.FakeCrc);
            command.Add(TestConstants.FakeCrc);

            SetTicketData ticketData = null;
            _handler.Setup(x => x.Handle(It.IsAny<SetTicketData>()))
                .Callback<SetTicketData>(data => ticketData = data)
                .Returns(() => new LongPollReadSingleValueResponse<TicketDataStatus>(response));
            var actualResponse = _target.Parse(command);

            CollectionAssert.AreEquivalent(expectedResponse, actualResponse.ToList());
            Assert.IsNotNull(ticketData);
            Assert.AreEqual(location, ticketData.Location);
            Assert.AreEqual(address1, ticketData.Address1);
            Assert.AreEqual(address2, ticketData.Address2);
            Assert.IsTrue(ticketData.IsExtendTicketData);
            Assert.AreEqual(restrictedTicketTitle, ticketData.RestrictedTicketTitle);
            Assert.AreEqual(debitTicketTitle, ticketData.DebitTicketTitle);
        }

        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                3,
                0xFF, // Invalid function code,
                0x01,
                0x65,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Invalid Function will NACK command")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                6,
                (byte)ExtendTicketDataCode.Location,
                0x01,
                0x65,
                (byte)ExtendTicketDataCode.Address1,
                0x02,
                0x65,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Invalid Data Length will NACK command")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                4,
                (byte)ExtendTicketDataCode.Location,
                0x01,
                0x65,
                (byte)ExtendTicketDataCode.Address1,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Missing data from for and extend ticket data code will NACK command")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                1,
                (byte)ExtendTicketDataCode.Location,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Not enough for one command will NACK command")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                43,
                (byte)ExtendTicketDataCode.Location,
                41, // Invalid data length
                // Data does not matter lets just send zero
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Exceeding 40 characters for location is invalid")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                43,
                (byte)ExtendTicketDataCode.Address1,
                41, // Invalid data length
                // Data does not matter lets just send zero
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Exceeding 40 characters for address1 is invalid")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                43,
                (byte)ExtendTicketDataCode.Address2,
                41, // Invalid data length
                // Data does not matter lets just send zero
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Exceeding 40 characters for address2 is invalid")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                19,
                (byte)ExtendTicketDataCode.DebitTicketTitle,
                17, // Invalid data length
                // Data does not matter lets just send zero
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Exceeding 16 characters for Debit title is invalid")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                19,
                (byte)ExtendTicketDataCode.RestrictedTicketTitle,
                17, // Invalid data length
                // Data does not matter lets just send zero
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Exceeding 16 characters for Restricted title is invalid")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                31, // Invalid length
                (byte)ExtendTicketDataCode.Location,
                0x01,
                0x65,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Incorrect length amount will fail command")]
        [DataTestMethod]
        public void InvalidDataParser(byte[] command)
        {
            var expectedResponse = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                (byte)TicketDataStatus.InvalidData
            };

            _handler.Setup(x => x.Handle(It.IsAny<SetTicketData>()))
                .Returns(() => new LongPollReadSingleValueResponse<TicketDataStatus>(TicketDataStatus.InvalidData));
            CollectionAssert.AreEquivalent(expectedResponse, _target.Parse(command).ToList());
        }
    }
}