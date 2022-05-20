namespace Aristocrat.SasClient.Tests.LPParsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP50SendValidationMetersParserTests
    {
        private LP50SendValidationMetersParser _target;

        private Mock<ISasLongPollHandler<SendValidationMetersResponse,
                LongPollSingleValueData<TicketValidationType>>> _handler;

        [TestInitialize]
        public void MyTestInitialization()
        {
            _handler = new Mock<ISasLongPollHandler<SendValidationMetersResponse,
                LongPollSingleValueData<TicketValidationType>>>(MockBehavior.Default);
            _target = new LP50SendValidationMetersParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendValidationMeters, _target.Command);
        }

        [DataRow((byte)0x1F, DisplayName = "Not a valid request type")]
        [DataRow((byte)TicketValidationType.None, DisplayName = "No validation type")]
        [DataTestMethod]
        public void InvalidDataParserTest(byte invalidCommand)
        {
            var commandData = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendValidationMeters,
                invalidCommand, // Invalid Request
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expectedResponse = new List<byte>
            {
                TestConstants.SasAddress | SasConstants.Nack
            };

            var response = _target.Parse(commandData);
            CollectionAssert.AreEquivalent(expectedResponse, response.ToList());
        }

        [TestMethod]
        public void ValidDataParserTest()
        {
            const long validationCount = 1254L;
            const long validationAmount = 165241152L;
            const TicketValidationType expectedValidationType = TicketValidationType.CashableTicketRedeemed;

            var commandData = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendValidationMeters,
                (byte)expectedValidationType,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            };

            var expectedResponse = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendValidationMeters,
                (byte)expectedValidationType,
                0x00, 0x00, 0x12, 0x54, // Count
                0x01, 0x65, 0x24, 0x11, 0x52 // Validation Amount
            };

            _handler.Setup(
                    x => x.Handle(
                        It.Is<LongPollSingleValueData<TicketValidationType>>(
                            data => data.Value == expectedValidationType)))
                .Returns(new SendValidationMetersResponse(validationCount, validationAmount)).Verifiable();
            var response = _target.Parse(commandData);
            CollectionAssert.AreEqual(expectedResponse, response.ToList());
            _handler.Verify();
        }
    }
}