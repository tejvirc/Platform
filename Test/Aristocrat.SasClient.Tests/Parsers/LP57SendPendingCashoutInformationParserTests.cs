namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LP57SendPendingCashoutInformationParserTests
    {
        private Mock<ISasLongPollHandler<SendPendingCashoutInformation, LongPollData>> _handler;
        private LP57SendPendingCashoutInformationParser _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler = new Mock<ISasLongPollHandler<SendPendingCashoutInformation, LongPollData>>(MockBehavior.Default);
            _target = new LP57SendPendingCashoutInformationParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SendPendingCashoutInformation, _target.Command);
        }

        [DataRow(
            new[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendPendingCashoutInformation
            },
            true,
            CashoutTypeCode.CashableTicket,
            1234567890UL,
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendPendingCashoutInformation,
                (byte)CashoutTypeCode.CashableTicket,
                0x12, 0x34, 0x56, 0x78, 0x90
            },
            DisplayName = "Valid Cashable Ticket Response")]
        [DataRow(
            new[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendPendingCashoutInformation
            },
            true,
            CashoutTypeCode.RestrictedPromotionalTicket,
            1234567890UL,
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendPendingCashoutInformation,
                (byte)CashoutTypeCode.RestrictedPromotionalTicket,
                0x12, 0x34, 0x56, 0x78, 0x90
            },
            DisplayName = "Valid Restricted Promotional Ticket Response")]
        [DataRow(
            new[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendPendingCashoutInformation
            },
            true,
            CashoutTypeCode.NotWaitingForSystemValidation,
            0Ul,
            new byte[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendPendingCashoutInformation,
                (byte)CashoutTypeCode.NotWaitingForSystemValidation,
                0x00, 0x00, 0x00, 0x00, 0x00
            },
            DisplayName = "EGM not waiting on validation")]
        [DataRow(
            new[]
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SendPendingCashoutInformation
            },
            false,
            CashoutTypeCode.NotWaitingForSystemValidation,
            0Ul,
            null,
            DisplayName = "No response test")]
        [DataTestMethod]
        public void ParseTest(
            byte[] commandData,
            bool validResponse,
            CashoutTypeCode cashoutType,
            ulong amount,
            byte[] expectedResults)
        {
            _handler.Setup(x => x.Handle(It.IsAny<LongPollData>()))
                .Returns(new SendPendingCashoutInformation(validResponse, cashoutType, amount));
            var result = _target.Parse(commandData);
            CollectionAssert.AreEquivalent(expectedResults, result?.ToList());
        }
    }
}