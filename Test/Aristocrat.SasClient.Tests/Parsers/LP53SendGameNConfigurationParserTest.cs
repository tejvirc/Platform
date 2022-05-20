namespace Aristocrat.SasClient.Tests.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP53SendGameNConfigurationParserTest
    {
        private LP53SendGameNConfigurationParser _target = new LP53SendGameNConfigurationParser(new SasClientConfiguration());
        private Mock<ISasLongPollHandler<LongPollMachineIdAndInfoResponse, LongPollGameNConfigurationData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<LongPollMachineIdAndInfoResponse, LongPollGameNConfigurationData>>(MockBehavior.Strict);
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendGameNConfiguration, _target.Command);
        }

        [DataRow("123456", "123", "123456", "123")]
        [DataRow("12345678910", "123456", "123456", "123")]
        [DataRow("123", "12", "123\0\0\0", "012")]
        [DataTestMethod]
        public void HandleValidTest(string payTableId, string additionalId, string expectedPaytableId, string expectedAdditionalId)
        {
            _handler.Setup(c => c.Handle(It.IsAny<LongPollGameNConfigurationData>()))
                .Returns(new LongPollMachineIdAndInfoResponse("AT", additionalId, 1, 0xC9, 0x12, 0, payTableId, "9050"));
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNConfiguration,
                0x12, 0x34 // Game number
            };

            var actual = _target.Parse(command).ToList();
            var expected = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNConfiguration,
                0x12, 0x34 // Game number
            };
            expected.AddRange(Encoding.ASCII.GetBytes("AT"));
            expected.AddRange(Encoding.ASCII.GetBytes(expectedAdditionalId));
            expected.Add(0x01);
            expected.Add(0xC9);
            expected.Add(0x12);
            expected.Add(0x00);
            expected.Add(0x00);
            expected.AddRange(Encoding.ASCII.GetBytes(expectedPaytableId));
            expected.AddRange(Encoding.ASCII.GetBytes("9050"));

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HandleInvalidGameNumberTest()
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendGameNConfiguration,
                0xAB, 0xCD // Game number
            };

            var actual = _target.Parse(command).ToList();
            var expected = new List<byte>
            {
                TestConstants.SasAddress | TestConstants.Nack
            };

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
