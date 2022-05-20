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
    public class LP70SendTicketValidationDataParserTest
    {
        private readonly Mock<ISasLongPollHandler<SendTicketValidationDataResponse, LongPollData>> _handler = new Mock<ISasLongPollHandler<SendTicketValidationDataResponse, LongPollData>>();
        private LP70SendTicketValidationDataParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP70SendTicketValidationDataParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
        }

        [TestMethod]
        public void GetNullResponseTest()
        {
            SetupHandlerMock(null);
            Assert.AreEqual(null, _target.Parse(new List<byte>() { TestConstants.SasAddress, 0x70 }));
        }

        [TestMethod]
        public void GetValidResponseWithBarcodeTest()
        {
            var response = new SendTicketValidationDataResponse()
            { ParsingCode = ParsingCode.Bcd, Barcode = "004054504974162392" };
            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(response);
            var expectedTxBytes = new byte[]
            {
                TestConstants.SasAddress, (byte)LongPoll.SendTicketValidationData,
                0x10, // 16 data bytes in length
                0x00, // status code (escrowed)
                0x00, 0x00, 0x00, 0x00, 0x00, // amount
                0x00, // parsing code
                0x00, 0x40, 0x54, 0x50, 0x49, 0x74, 0x16, 0x23, 0x92, // validation data
            };

            var actualTxBytes = _target.Parse(new List<byte>() { TestConstants.SasAddress, 0x70 });
            Assert.IsTrue(expectedTxBytes.SequenceEqual(actualTxBytes.ToArray()));
        }

        [TestMethod]
        public void GetValidResponseWithEmptyBarcodeTest()
        {
            var response = new SendTicketValidationDataResponse()
            { ParsingCode = ParsingCode.Bcd, Barcode = string.Empty };
            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(response);
            var expectedTxBytes = new byte[]
            {
                TestConstants.SasAddress, (byte)LongPoll.SendTicketValidationData,
                0x01, // 1 byte in length
                0xFF, // status code (escrowed)
            };

            var actualTxBytes = _target.Parse(new List<byte>() { TestConstants.SasAddress, 0x70 });
            Assert.IsTrue(expectedTxBytes.SequenceEqual(actualTxBytes.ToArray()));
        }

        private void SetupHandlerMock(SendTicketValidationDataResponse response)
        {
            _handler.Setup(m => m.Handle(It.IsAny<LongPollData>())).Returns(response);
        }
    }
}
