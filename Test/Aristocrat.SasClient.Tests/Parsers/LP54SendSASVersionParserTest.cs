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
    public class LP54SendSasVersionParserTest
    {
        private LP54SendSasVersionParser _target;
        private Mock<ISasLongPollHandler<LongPollSendSasVersionResponse, LongPollData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP54SendSasVersionParser();
            _handler = new Mock<ISasLongPollHandler<LongPollSendSasVersionResponse, LongPollData>>(MockBehavior.Default);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendSasVersionAndGameSerial, _target.Command);
        }

        [DataRow(
            "603",
            "1234567890123456789012345678901234567890",
            "603",
            "1234567890123456789012345678901234567890",
            DisplayName = "Max sending data test")]
        [DataRow("6", "1234567890", "6", "1234567890", DisplayName = "Sending less than max")]
        [DataRow(
            "6037890",
            "12345678901234567890123456789012345678901234567890",
            "603",
            "1234567890123456789012345678901234567890",
            DisplayName = "Sending more than max will truncate")]
        [DataTestMethod]
        public void ParseSucceedTest(
            string version,
            string serialNumber,
            string expectedVersion,
            string expectedSerialNumber)
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSasVersionAndGameSerial
            };

            var response = new LongPollSendSasVersionResponse(version, serialNumber);

            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(response);

            var expected = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SendSasVersionAndGameSerial,
            };

            var versionBytes = System.Text.Encoding.UTF8.GetBytes(expectedVersion);
            var serialNumberBytes = System.Text.Encoding.UTF8.GetBytes(expectedSerialNumber);

            expected.Add((byte)(versionBytes.Length + serialNumberBytes.Length));
            expected.AddRange(versionBytes);
            expected.AddRange(serialNumberBytes);

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
