namespace Aristocrat.SasClient.Tests.Parsers.Eft
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.Eft;
    using Sas.Client.EFT.Response;

    [TestClass]
    public class LP66SendLastCashoutCreditAmountParserTests
    {
        private const byte FalseAck = 0x00;
        private const byte TrueAck = 0x01;
        private LP66SendLastCashoutCreditAmountParser _target;
        private Mock<ISasLongPollHandler<EftSendLastCashoutResponse, EftSendLastCashoutData>> _handler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _handler =
                new Mock<ISasLongPollHandler<EftSendLastCashoutResponse, EftSendLastCashoutData>>(MockBehavior.Strict);
            _handler.Setup(m => m.Handle(It.IsAny<EftSendLastCashoutData>()))
                .Returns(It.IsAny<EftSendLastCashoutResponse>());
            _target = new LP66SendLastCashoutCreditAmountParser();
            _target.InjectHandler(_handler.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.EftSendLastCashOutCreditAmount, _target.Command);
        }

        [DataRow(true, 0ul)]
        [DataRow(false, 200ul)]
        [DataRow(true, 0ul)]
        [DataRow(false, 200ul)]
        [DataTestMethod]
        public void ParseTest(bool ack, ulong amount)
        {
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.EftSendLastCashOutCreditAmount, ack ? TrueAck : FalseAck
            };

            _handler.Setup(c => c.Handle(It.IsAny<EftSendLastCashoutData>())).Returns(
                new EftSendLastCashoutResponse
                {
                    Acknowledgement = ack,
                    Status = TransactionStatus.OperationSuccessful,
                    LastCashoutAmount = amount
                });

            var actualResult = _target.Parse(command).ToArray();

            var expectedResults = new List<byte>();
            expectedResults.AddRange(command);
            expectedResults.Add((byte)TransactionStatus.OperationSuccessful);
            expectedResults.AddRange(Utilities.ToBcd(amount, SasConstants.Bcd10Digits));
            CollectionAssert.AreEqual(expectedResults, actualResult.ToList());
        }
    }
}