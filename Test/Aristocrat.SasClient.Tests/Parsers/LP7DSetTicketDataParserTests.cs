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
    public class LP7DSetTicketDataParserTests
    {
        private LP7DSetTicketDataParser _target;
        private Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<TicketDataStatus>, SetTicketData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP7DSetTicketDataParser();
            _handler =
                new Mock<ISasLongPollHandler<LongPollReadSingleValueResponse<TicketDataStatus>, SetTicketData>>(
                    MockBehavior.Default);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SetTicketData, _target.Command);
        }

        [DataRow(
            "location",
            "address1",
            "address2",
            0,
            213,
            TicketDataStatus.ValidData,
            DisplayName = "All values can be sent")]
        [DataRow(
            "1234567890123456789012345678901234567890",
            null,
            null,
            0,
            213,
            TicketDataStatus.ValidData,
            DisplayName = "Upt to Location is valid")]
        [DataRow(
            "1234567890123456789012345678901234567890",
            "1234567890123456789012345678901234567890",
            null,
            0,
            213,
            TicketDataStatus.ValidData,
            DisplayName = "Up to Address1 is valid")]
        [DataRow(
            "",
            "",
            "",
            0,
            213,
            TicketDataStatus.ValidData,
            DisplayName = "Empty Values are valid")]
        [DataRow(
            null,
            null,
            null,
            -1,
            213,
            TicketDataStatus.ValidData,
            DisplayName = "Just host Id is valid")]
        [DataRow(
            null,
            null,
            null,
            0,
            213,
            TicketDataStatus.ValidData,
            DisplayName = "Up to expiration date is valid")]
        [DataRow(
            null,
            null,
            null,
            -1,
            -1,
            TicketDataStatus.InvalidData,
            DisplayName = "No length is valid")]
        [DataTestMethod]
        public void ValidParserTest(
            string location,
            string address1,
            string address2,
            int expirationDate,
            int hostId,
            TicketDataStatus response)
        {
            var expectedResponse = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetTicketData,
                (byte) response
            };

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetTicketData,
                0, // Set the length later
            };

            if (hostId >= 0)
            {
                command.AddRange(Utilities.ToBinary((uint)hostId, 2));
            }

            if (expirationDate >= 0)
            {
                command.Add((byte)expirationDate);
            }

            if (location != null)
            {
                command.Add((byte)location.Length);
                command.AddRange(Encoding.ASCII.GetBytes(location));
            }

            if (address1 != null)
            {
                command.Add((byte)address1.Length);
                command.AddRange(Encoding.ASCII.GetBytes(address1));
            }

            if (address2 != null)
            {
                command.Add((byte)address2.Length);
                command.AddRange(Encoding.ASCII.GetBytes(address2));
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
            Assert.IsTrue(ticketData.ValidTicketData);
            Assert.AreEqual(string.IsNullOrEmpty(location) ? null : location, ticketData.Location);
            Assert.AreEqual(string.IsNullOrEmpty(address1) ? null : address1, ticketData.Address1);
            Assert.AreEqual(string.IsNullOrEmpty(address2) ? null : address2, ticketData.Address2);
            Assert.IsFalse(ticketData.IsExtendTicketData);
            if (hostId >= 0)
            {
                Assert.AreEqual(hostId, ticketData.HostId);
            }

            if (expirationDate >= 0)
            {
                Assert.IsTrue(ticketData.SetExpirationDate);
                Assert.AreEqual(expirationDate, ticketData.ExpirationDate);
            }
            else
            {
                Assert.IsFalse(ticketData.SetExpirationDate);
            }
        }

        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                0x01,
                0x00, // Host ID missing one byte
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Incorrect Host ID length")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                0x06,
                0x00, 0x31, // Host ID
                0x21, // Expiration Date
                0x03, // Invalid location length
                0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Incorrect location length")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                0x07,
                0x00, 0x31, // Host ID
                0x21, // Expiration Date
                0x00, // Location Length
                0x03, // Invalid Address 1 length
                0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Incorrect Address 1 length")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                0x08,
                0x00, 0x31, // Host ID
                0x21, // Expiration Date
                0x00, // Location Length
                0x00, // Address 1 length
                0x03, // Invalid Address 2 length
                0x00, 0x00,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Incorrect Address 2 length")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                0x08,
                0x00, 0x31, // Host ID
                0x21, // Expiration Date
                0x00, // Location Length
                0x00, // Address 1 length
                0x00, // Address 2 length
                0x00, 0x00, // Invalid Extra Data
                TestConstants.FakeCrc, TestConstants.FakeCrc
            }, DisplayName = "Incorrect extra unused data length")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetExtendedTicketData,
                31, // Invalid length
                0x00, 0x00, // Host ID
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