namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP58ReceiveValidationNumberParserTests
    {
        private Mock<ISasLongPollHandler<ReceiveValidationNumberResult, ReceiveValidationNumberData>> _handler;
        private LP58ReceiveValidationNumberParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<ReceiveValidationNumberResult, ReceiveValidationNumberData>>(MockBehavior.Default);
            _target = new LP58ReceiveValidationNumberParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.ReceiveValidationNumber, _target.Command);
        }

        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ReceiveValidationNumber,
                0x01, 
                0x12, 0x34, 0x56, 0x78, 0x90, 0x12, 0x34, 0x56, // Validation Number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            },
            (byte)0x01,
            1234567890123456UL,
            true,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            new[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ReceiveValidationNumber,
                (byte)ReceiveValidationNumberStatus.CommandAcknowledged
            },
            DisplayName = "Valid Command with response")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ReceiveValidationNumber,
                0x01,
                0x12, 0xA4, 0x56, 0x78, 0x90, 0x12, 0x34, 0x56, // Validation Number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            },
            (byte)0x01,
            1234567890123456UL,
            true,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            new[]
            {
                (byte)(TestConstants.SasAddress | SasConstants.Nack)
            },
            DisplayName = "Invalid Validation Number BCD Received")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ReceiveValidationNumber,
                0xA1,
                0x12, 0x34, 0x56, 0x78, 0x90, 0x12, 0x34, 0x56, // Validation Number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            },
            (byte)0x01,
            1234567890123456UL,
            true,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            new[]
            {
                (byte)(TestConstants.SasAddress | SasConstants.Nack)
            },
            DisplayName = "Invalid System ID BCD Received")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ReceiveValidationNumber,
                0x01,
                0x12, 0x34, 0x56, 0x78, 0x90, 0x12, 0x34
            },
            (byte)0x01,
            1234567890123456UL,
            true,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            new[]
            {
                (byte)(TestConstants.SasAddress | SasConstants.Nack)
            },
            DisplayName = "Invalid Length Received")]
        [DataRow(
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.ReceiveValidationNumber,
                0x01,
                0x12, 0x34, 0x56, 0x78, 0x90, 0x12, 0x34, 0x56, // Validation Number
                TestConstants.FakeCrc, TestConstants.FakeCrc
            },
            (byte)0x01,
            1234567890123456UL,
            false,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            null,
            DisplayName = "Valid Command without any response")]
        [DataTestMethod]
        public void ParseTest(
            byte[] commandData,
            byte expectedSystemId,
            ulong expectedValidationNumber,
            bool validResponse,
            ReceiveValidationNumberStatus status,
            byte[] expectedResults)
        {
            _handler.Setup(
                    x => x.Handle(
                        It.Is<ReceiveValidationNumberData>(
                            res => res.ValidationSystemId == expectedSystemId &&
                                   res.ValidationNumber == expectedValidationNumber)))
                .Returns(new ReceiveValidationNumberResult(validResponse, status));
            var result = _target.Parse(commandData);
            CollectionAssert.AreEquivalent(expectedResults, result?.ToList());
        }
    }
}