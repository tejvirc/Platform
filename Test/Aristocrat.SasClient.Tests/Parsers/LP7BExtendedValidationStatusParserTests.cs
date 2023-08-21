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
    public class LP7BExtendedValidationStatusParserTests
    {
        private static IEnumerable<object[]> ExtendValidationParserData => new List<object[]>
        {
            new object[]
            {
                new byte[]
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.ExtendedValidationStatus,
                    0x08,
                    0x3F, 0x80,
                    0x3F, 0x80,
                    0x99, 0x99,
                    0x09, 0x99,
                    TestConstants.FakeCrc, TestConstants.FakeCrc
                },
                new byte[]
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.ExtendedValidationStatus,
                    0x0A,
                    0x7B, 0x00, 0x00, 0x00,
                    0x3F, 0x80,
                    0x99, 0x99,
                    0x09, 0x99
                },
                new ExtendedValidationStatusResponse(
                    123,
                    ValidationControlStatus.TicketRedemption | ValidationControlStatus.PrintForeignRestrictedTickets |
                    ValidationControlStatus.PrintRestrictedTickets | ValidationControlStatus.UsePrinterAsCashoutDevice |
                    ValidationControlStatus.UsePrinterAsHandPayDevice | ValidationControlStatus.ValidateHandPays |
                    ValidationControlStatus.SecureEnhancedConfiguration,
                    9999,
                    999)
            },
            new object[]
            {
                new byte[]
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.ExtendedValidationStatus,
                    0x08,
                    0x07, 0x00,
                    0x01, 0x00,
                    0x99, 0x99,
                    0x09, 0x99,
                    TestConstants.FakeCrc, TestConstants.FakeCrc
                },
                new byte[]
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.ExtendedValidationStatus,
                    0x0A,
                    0x7B, 0x00, 0x00, 0x00,
                    0x01, 0x00,
                    0x99, 0x99,
                    0x09, 0x99
                },
                new ExtendedValidationStatusResponse(
                    123,
                    ValidationControlStatus.UsePrinterAsCashoutDevice,
                    9999,
                    999)
            },
            new object[]
            {
                new byte[]
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.ExtendedValidationStatus,
                    0x08,
                    0xFF, 0xFF,
                    0xFF, 0xFF,
                    0x99, 0x99,
                    0x09, 0x99,
                    TestConstants.FakeCrc, TestConstants.FakeCrc
                },
                new byte[]
                {
                    TestConstants.SasAddress,
                    (byte)LongPoll.ExtendedValidationStatus,
                    0x0A,
                    0x7B, 0x00, 0x00, 0x00,
                    0x3F, 0x80,
                    0x99, 0x99,
                    0x09, 0x99
                },
                new ExtendedValidationStatusResponse(
                    123,
                    ValidationControlStatus.PrintForeignRestrictedTickets |
                    ValidationControlStatus.PrintForeignRestrictedTickets |
                    ValidationControlStatus.PrintRestrictedTickets | ValidationControlStatus.TicketRedemption |
                    ValidationControlStatus.UsePrinterAsCashoutDevice |
                    ValidationControlStatus.UsePrinterAsHandPayDevice |
                    ValidationControlStatus.ValidateHandPays |
                    ValidationControlStatus.SecureEnhancedConfiguration,
                    9999,
                    999)
            }
        };

        private LP7BExtendedValidationStatusParser _target;
        private Mock<ISasLongPollHandler<ExtendedValidationStatusResponse, ExtendedValidationStatusData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler =
                new Mock<ISasLongPollHandler<ExtendedValidationStatusResponse, ExtendedValidationStatusData>>(
                    MockBehavior.Default);
            _target = new LP7BExtendedValidationStatusParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.ExtendedValidationStatus, _target.Command);
        }

        [DynamicData(nameof(ExtendValidationParserData))]
        [DataTestMethod]
        public void ValidParseTest(byte[] command, byte[] expectedResponse, ExtendedValidationStatusResponse response)
        {
            _handler.Setup(x => x.Handle(It.IsAny<ExtendedValidationStatusData>())).Returns(response);

            var actualResponse = _target.Parse(command);
            CollectionAssert.AreEquivalent(expectedResponse, actualResponse.ToList());
        }

        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ExtendedValidationStatus,
                0x08,
                0x3F, 0x80,
                0x3F, 0x80,
                0x9A, 0x99,
                0x09, 0x99,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            })]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ExtendedValidationStatus,
                0x08,
                0x3F, 0x80,
                0x3F, 0x80,
                0x99, 0x99,
                0xA9, 0x99,
                TestConstants.FakeCrc, TestConstants.FakeCrc
            })]
        [DataTestMethod]
        public void InvalidParseTest(byte[] command)
        {
            var expectedResult = new List<byte>
            {
                TestConstants.SasAddress | SasConstants.Nack
            };

            var result = _target.Parse(command);
            CollectionAssert.AreEquivalent(expectedResult, result.ToList());
        }
    }
}